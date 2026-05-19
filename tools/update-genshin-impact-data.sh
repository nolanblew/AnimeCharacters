#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
export SCRIPT_DIR

python3 - "$@" <<'PY'
import argparse
import html
import json
import os
import re
import sys
import time
import unicodedata
import urllib.error
import urllib.parse
import urllib.request

DEFAULT_OUTPUT = os.path.abspath(os.path.join(
    os.environ["SCRIPT_DIR"],
    "..",
    "AnimeCharacters",
    "wwwroot",
    "data",
    "extensions",
    "genshin-impact-characters.json"))
DEFAULT_IMAGE_OUTPUT_DIR = os.path.abspath(os.path.join(
    os.environ["SCRIPT_DIR"],
    "..",
    "AnimeCharacters",
    "wwwroot",
    "images",
    "extensions",
    "genshin-impact"))

parser = argparse.ArgumentParser(description="Refresh checked-in Genshin Impact extension data.")
parser.add_argument("-o", "--output", default=DEFAULT_OUTPUT)
parser.add_argument("--resolve-anilist-ids", action="store_true")
parser.add_argument("--request-delay-ms", type=int, default=700)
parser.add_argument("--image-output-dir", default=DEFAULT_IMAGE_OUTPUT_DIR)
parser.add_argument("--image-url-prefix", default="images/extensions/genshin-impact")
parser.add_argument("--refresh-images", action="store_true")
args = parser.parse_args()

last_request_at = None


def write_progress(message):
    print(message, file=sys.stderr, flush=True)


def request_json(url, method="GET", body=None, status="Requesting API data"):
    global last_request_at

    if last_request_at is not None:
        elapsed_ms = (time.time() - last_request_at) * 1000
        remaining_ms = args.request_delay_ms - elapsed_ms
        if remaining_ms > 0:
            time.sleep(remaining_ms / 1000)

    write_progress(status)
    data = body.encode("utf-8") if body is not None else None
    headers = {
        "User-Agent": "AnimeCharactersDataUpdater/1.0 (https://www.animecharacters.app/)"
    }
    if body is not None:
        headers["Content-Type"] = "application/json"

    request = urllib.request.Request(url, data=data, headers=headers, method=method)
    try:
        with urllib.request.urlopen(request, timeout=30) as response:
            return json.loads(response.read().decode("utf-8"))
    finally:
        last_request_at = time.time()


def fragment_to_text(markup):
    if not markup or not markup.strip():
        return None

    without_references = re.sub(r"<sup[\s\S]*?</sup>", "", markup)
    with_spaces = re.sub(r"</?(br|p)[^>]*>", " ", without_references)
    without_tags = re.sub(r"<[^>]+>", "", with_spaces)
    decoded = html.unescape(without_tags)
    return re.sub(r"\s+", " ", decoded).strip()


def image_url(url):
    if not url or not url.strip():
        return None

    decoded = html.unescape(url)
    return re.sub(r"/revision/latest/scale-to-width-down/\d+", "/revision/latest/", decoded)


def asset_file_name(name):
    normalized = normalized_name(name)
    if not normalized:
        return "unknown"

    return re.sub(r"\s+", "-", normalized)


def download_image(url, output_path, status):
    global last_request_at

    if not url:
        return
    # Existing assets are reused by default, but missing assets are always downloaded
    # so newly added characters get images without requiring --refresh-images.
    if os.path.exists(output_path) and not args.refresh_images:
        return

    if last_request_at is not None:
        elapsed_ms = (time.time() - last_request_at) * 1000
        remaining_ms = args.request_delay_ms - elapsed_ms
        if remaining_ms > 0:
            time.sleep(remaining_ms / 1000)

    write_progress(status)
    os.makedirs(os.path.dirname(output_path), exist_ok=True)
    request = urllib.request.Request(
        url,
        headers={"User-Agent": "AnimeCharactersDataUpdater/1.0 (https://www.animecharacters.app/)"})
    try:
        with urllib.request.urlopen(request, timeout=30) as response, open(output_path, "wb") as output:
            output.write(response.read())
    finally:
        last_request_at = time.time()


def normalized_name(name):
    if not name or not name.strip():
        return None

    decomposed = unicodedata.normalize("NFD", name)
    without_marks = "".join(
        char for char in decomposed
        if unicodedata.category(char) != "Mn")
    plain = unicodedata.normalize("NFC", without_marks).lower()
    return re.sub(r"[^\w]+", " ", plain, flags=re.UNICODE).strip()


def name_matches(expected, candidate):
    expected_normalized = normalized_name(expected)
    candidate_normalized = normalized_name(candidate)

    if not expected_normalized or not candidate_normalized:
        return False
    if expected_normalized == candidate_normalized:
        return True

    parts = expected_normalized.split()
    return len(parts) == 2 and f"{parts[1]} {parts[0]}" == candidate_normalized


def voice_actor_from_link(link_markup):
    text = fragment_to_text(link_markup)
    if not text:
        return None

    name = text
    native_name = None
    match = re.match(r"^(?P<name>.*?)\s*\((?P<native>[^()]*)\)", text)
    if match:
        name = match.group("name").strip()
        native_name = match.group("native").strip()

    return {
        "Name": name,
        "NativeName": native_name,
        "AniListStaffId": None,
    }


def voice_actor_variant_label(before_link_markup, after_link_markup):
    before_text = fragment_to_text(before_link_markup)
    if before_text:
        label = before_text.strip().rstrip(":").strip()
        if label:
            return label

    after_text = fragment_to_text(after_link_markup)
    if after_text:
        match = re.match(r"^\((?P<label>[^()]+)\)", after_text.strip())
        if match:
            return match.group("label").strip()

    return None


def voice_actor_entries(cell_markup):
    without_references = re.sub(r"<sup[\s\S]*?</sup>", "", cell_markup)
    segments = re.split(r"(?i)<br\s*/?>", without_references)
    voice_actors = []

    for segment in segments:
        for voice_actor_link in re.finditer(r"<a [^>]*>[\s\S]*?</a>", segment):
            voice_actor = voice_actor_from_link(voice_actor_link.group(0))
            if not voice_actor or not voice_actor.get("Name"):
                continue

            voice_actor["CharacterVariantLabel"] = voice_actor_variant_label(
                segment[:voice_actor_link.start()],
                segment[voice_actor_link.end():])
            voice_actors.append(voice_actor)

    return voice_actors


def character_display_name(character_name, variant_label, voice_actor_count):
    if voice_actor_count <= 1 or not variant_label:
        return character_name

    if character_name == "Traveler":
        if variant_label == "Aether":
            return "Traveler (Male)"
        if variant_label == "Lumine":
            return "Traveler (Female)"

    if "(" in variant_label or ")" in variant_label:
        return f"{character_name} - {variant_label}"

    return f"{character_name} ({variant_label})"


def character_icon_urls():
    url = "https://genshin-impact.fandom.com/api.php?action=parse&page=Character/List&prop=text&format=json&formatversion=2"
    response = request_json(url, status="Fetching Character/List icon data from Fandom")
    wiki_html = response["parse"]["text"]
    icons = {}
    rows = re.findall(r"<tr\b[\s\S]*?</tr>", wiki_html)

    for index, row in enumerate(rows, start=1):
        if index % 25 == 0:
            write_progress(f"Parsing Fandom character icons {index}/{len(rows)}")

        name_match = re.search(r'<td\b[^>]*\bdata-name="(?P<name>[^"]+)"', row)
        if not name_match:
            continue

        icon_match = re.search(
            r'<img\b(?=[^>]*\bdata-image-key="[^"]+_Icon\.png")[^>]*\bdata-src="(?P<src>[^"]+)"',
            row)
        if not icon_match:
            icon_match = re.search(r'<img\b[^>]*\bdata-src="(?P<src>[^"]+)"', row)
        if not icon_match:
            continue

        name = html.unescape(name_match.group("name"))
        icons.setdefault(name, image_url(icon_match.group("src")))

    return icons


def resolve_anilist_staff_id(voice_actor):
    queries = []
    for query in (voice_actor.get("NativeName"), voice_actor.get("Name")):
        if query and query.strip() and query not in queries:
            queries.append(query)

    gql = """
query searchStaff($search: String) {
  Page(page: 1, perPage: 5) {
    staff(search: $search, sort: SEARCH_MATCH) {
      id
      languageV2
      name {
        full
        native
      }
    }
  }
}
"""

    for query in queries:
        body = json.dumps({"query": gql, "variables": {"search": query}})
        try:
            result = request_json(
                "https://graphql.anilist.co",
                method="POST",
                body=body,
                status=f"Resolving AniList staff: {query}")
        except urllib.error.URLError:
            continue

        for staff in result.get("data", {}).get("Page", {}).get("staff", []):
            if staff.get("languageV2") != "JAPANESE":
                continue
            staff_name = staff.get("name") or {}
            if name_matches(voice_actor.get("Name"), staff_name.get("full")) or name_matches(voice_actor.get("NativeName"), staff_name.get("native")):
                return int(staff["id"])

    return None


icons_by_name = character_icon_urls()
cover_source_url = "https://static.wikia.nocookie.net/gensin-impact/images/8/80/Genshin_Impact.png/revision/latest/scale-to-width-down/512?cb=20240331104358"
download_image(
    cover_source_url,
    os.path.join(args.image_output_dir, "cover.png"),
    "Downloading Genshin Impact cover")
character_image_dir = os.path.join(args.image_output_dir, "characters")

voice_actor_url = "https://genshin-impact.fandom.com/api.php?action=parse&page=Voice_Actor&prop=text&format=json&formatversion=2"
voice_actor_response = request_json(voice_actor_url, status="Fetching Voice Actor table from Fandom")
voice_actor_html = voice_actor_response["parse"]["text"]
table_match = re.search(r"<table[^>]*wikitable[\s\S]*?</table>", voice_actor_html)
if not table_match:
    raise RuntimeError("Could not find the playable character voice actor table.")

characters = []
rows = re.findall(r"<tr[\s\S]*?</tr>", table_match.group(0))[1:]
for index, row in enumerate(rows, start=1):
    if index % 25 == 0:
        write_progress(f"Parsing voice actor rows {index}/{len(rows)}")

    cells = re.findall(r"<td[^>]*>[\s\S]*?</td>", row)
    if len(cells) < 4:
        continue

    character_link = re.search(r'<a href="(?P<href>/wiki/[^"]+)" title="(?P<title>[^"]+)"[^>]*>[\s\S]*?</a>', cells[0])
    if not character_link:
        continue

    character_name = html.unescape(character_link.group("title"))
    voice_actors = voice_actor_entries(cells[3])

    if not voice_actors:
        continue

    source_image_url = icons_by_name.get(character_name)
    local_image_url = None
    if source_image_url:
        file_name = f"{asset_file_name(character_name)}.png"
        download_image(
            source_image_url,
            os.path.join(character_image_dir, file_name),
            f"Downloading {character_name} icon")
        local_image_url = f"{args.image_url_prefix}/characters/{file_name}"

    for voice_actor in voice_actors:
        variant_label = voice_actor.pop("CharacterVariantLabel", None)
        characters.append({
            "Name": character_display_name(character_name, variant_label, len(voice_actors)),
            "ImageUrl": local_image_url,
            "WikiUrl": f"https://genshin-impact.fandom.com{character_link.group('href')}",
            "JapaneseVoiceActors": [voice_actor],
        })

if args.resolve_anilist_ids:
    cache = {}
    work_items = [actor for character in characters for actor in character["JapaneseVoiceActors"]]
    total = max(len(work_items), 1)
    current = 0

    for character in characters:
        for voice_actor in character["JapaneseVoiceActors"]:
            current += 1
            cache_key = voice_actor.get("NativeName") or voice_actor.get("Name")
            write_progress(f"Resolving AniList staff IDs {current}/{total}: {voice_actor.get('Name')}")
            if cache_key not in cache:
                cache[cache_key] = resolve_anilist_staff_id(voice_actor)
            voice_actor["AniListStaffId"] = cache[cache_key]

characters.sort(key=lambda item: item["Name"])
os.makedirs(os.path.dirname(args.output), exist_ok=True)
with open(args.output, "w", encoding="utf-8") as handle:
    json.dump(characters, handle, ensure_ascii=False, indent=2)
    handle.write("\n")

missing_images = sum(1 for character in characters if not character.get("ImageUrl"))
print(f"Wrote {len(characters)} Genshin Impact characters to {args.output}")
print(f"Missing character icon URLs: {missing_images}")
PY
