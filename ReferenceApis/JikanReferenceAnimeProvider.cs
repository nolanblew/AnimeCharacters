using AniListClient.Models;
using Kitsu.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReferenceApis
{
    public class JikanReferenceAnimeProvider : IReferenceAnimeProvider
    {
        const string _BASE_URL = "https://api.jikan.moe/v4/";
        readonly HttpClient _httpClient;

        public JikanReferenceAnimeProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public string Name => ReferenceProviderNames.Jikan;
        public string DisplayName => "Jikan / MyAnimeList";

        public ReferenceAnimeKey GetKnownAnimeKey(Anime anime) =>
            !string.IsNullOrWhiteSpace(anime?.MyAnimeListId)
                ? new ReferenceAnimeKey(Name, anime.MyAnimeListId)
                : null;

        public async Task<ReferenceMediaResult> GetMediaWithCharactersAsync(
            Anime anime,
            IReadOnlyCollection<string> searchTitles)
        {
            var animeId = anime?.MyAnimeListId;

            if (string.IsNullOrWhiteSpace(animeId))
            {
                animeId = await SearchAnimeIdAsync(searchTitles);
            }

            if (string.IsNullOrWhiteSpace(animeId) || !int.TryParse(animeId, out var id))
            {
                throw new ReferenceApiProviderException("MyAnimeList does not have a matching anime id.");
            }

            var response = await GetFromJsonAsync<JikanDataResponse<List<JikanAnimeCharacterEntry>>>(
                $"anime/{id}/characters");

            var characters = response?.Data?
                .Select(ToCharacter)
                .Where(character => character != null)
                .ToList() ?? new List<Character>();

            return new ReferenceMediaResult(
                new ReferenceAnimeKey(Name, animeId),
                new Media(
                    Id: id,
                    Title: ToTitles(anime),
                    Description: null,
                    Image: ToImages(anime?.PosterImageUrl),
                    Status: MediaStatus.Finished,
                    Characters: characters,
                    ProviderName: Name));
        }

        public async Task<Staff> GetStaffByIdAsync(string id)
        {
            if (!int.TryParse(id, out var staffId))
            {
                throw new ReferenceApiProviderException("MyAnimeList person ids must be numeric.");
            }

            var response = await GetFromJsonAsync<JikanDataResponse<JikanPerson>>($"people/{staffId}/full");
            var person = response?.Data;

            if (person == null)
            {
                throw new ReferenceApiProviderException("MyAnimeList did not return person data.");
            }

            return new Staff(
                Id: person.MalId,
                Name: ToNames(person.Name),
                Language: Language.Japanese,
                Images: ToImages(person.Images?.Jpg?.ImageUrl),
                Description: person.About,
                Age: null,
                DateOfBirth: ToDateOfBirth(person.Birthday),
                BloodType: ExtractBloodType(person.About),
                SiteUrl: person.Url,
                Characters: person.Voices?.Select(ToCharacter).Where(character => character != null).ToList() ?? new List<Character>(),
                ProviderName: Name);
        }

        async Task<string> SearchAnimeIdAsync(IReadOnlyCollection<string> searchTitles)
        {
            foreach (var title in searchTitles ?? Array.Empty<string>())
            {
                var response = await GetFromJsonAsync<JikanDataResponse<List<JikanAnimeSearchResult>>>(
                    $"anime?q={Uri.EscapeDataString(title)}&limit=5");

                var match = response?.Data?.FirstOrDefault(result => _IsTitleMatch(result, searchTitles));
                if (match != null)
                {
                    return match.MalId.ToString();
                }
            }

            return null;
        }

        async Task<T> GetFromJsonAsync<T>(string relativeUrl)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<T>(new Uri(new Uri(_BASE_URL), relativeUrl));
            }
            catch (HttpRequestException ex)
            {
                throw new ReferenceApiProviderException("Jikan could not be reached.", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new ReferenceApiProviderException("Jikan request timed out.", ex);
            }
        }

        Character ToCharacter(JikanAnimeCharacterEntry entry)
        {
            if (entry?.Character == null)
            {
                return null;
            }

            var voiceActors = entry.VoiceActors?
                .Where(voiceActor => string.Equals(voiceActor.Language, "Japanese", StringComparison.OrdinalIgnoreCase))
                .Select(ToVoiceActorSlim)
                .Where(voiceActor => voiceActor != null)
                .ToList() ?? new List<VoiceActorSlim>();

            return new Character(
                Id: entry.Character.MalId,
                Name: ToNames(entry.Character.Name),
                Image: ToImages(entry.Character.Images?.Jpg?.ImageUrl),
                Description: null,
                Role: ToCharacterRole(entry.Role),
                Media: null,
                VoiceActors: voiceActors,
                ProviderName: Name);
        }

        Character ToCharacter(JikanPersonVoice entry)
        {
            if (entry?.Character == null || entry.Anime == null)
            {
                return null;
            }

            return new Character(
                Id: entry.Character.MalId,
                Name: ToNames(entry.Character.Name),
                Image: ToImages(entry.Character.Images?.Jpg?.ImageUrl),
                Description: null,
                Role: ToCharacterRole(entry.Role),
                Media: new List<MediaBase>
                {
                    new MediaBase(
                        Id: entry.Anime.MalId,
                        Title: ToTitles(entry.Anime.Title),
                        ProviderName: Name)
                },
                VoiceActors: null,
                ProviderName: Name);
        }

        VoiceActorSlim ToVoiceActorSlim(JikanVoiceActor voiceActor)
        {
            if (voiceActor?.Person == null)
            {
                return null;
            }

            return new VoiceActorSlim(
                Id: voiceActor.Person.MalId,
                Name: ToNames(voiceActor.Person.Name),
                ProviderName: Name,
                SiteUrl: voiceActor.Person.Url);
        }

        static CharacterRole? ToCharacterRole(string role) =>
            role?.Trim().ToLowerInvariant() switch
            {
                "main" => CharacterRole.Main,
                "supporting" => CharacterRole.Supporting,
                "background" => CharacterRole.Background,
                _ => null
            };

        static Names ToNames(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new Names(null, null, null, null, null, null, null);
            }

            var trimmedName = name.Trim();
            var first = trimmedName;
            string last = null;

            if (trimmedName.Contains(','))
            {
                var parts = trimmedName.Split(',', 2, StringSplitOptions.TrimEntries);
                last = parts[0];
                first = parts.Length > 1 ? parts[1] : null;
                trimmedName = string.Join(" ", new[] { first, last }.Where(part => !string.IsNullOrWhiteSpace(part)));
            }

            return new Names(
                Romaji: trimmedName,
                First: first,
                Last: last,
                Full: trimmedName,
                Native: null,
                Alternative: null,
                AlternativeSpoiler: null);
        }

        static Titles ToTitles(Anime anime) =>
            new Titles(
                Romaji: anime?.RomanjiTitle ?? anime?.Title,
                English: anime?.EnglishTitle,
                Native: null,
                UserPreferred: anime?.Title ?? anime?.RomanjiTitle ?? anime?.EnglishTitle);

        static Titles ToTitles(string title) =>
            new Titles(
                Romaji: title,
                English: title,
                Native: null,
                UserPreferred: title);

        static Images ToImages(string imageUrl) =>
            new Images(
                Medium: imageUrl,
                Large: imageUrl,
                ExtraLarge: imageUrl,
                Color: null);

        static DateOfBirth ToDateOfBirth(string birthday)
        {
            if (!DateTimeOffset.TryParse(birthday, out var date))
            {
                return null;
            }

            return new DateOfBirth(date.Year, date.Month, date.Day);
        }

        static string ExtractBloodType(string about)
        {
            if (string.IsNullOrWhiteSpace(about))
            {
                return null;
            }

            var match = Regex.Match(about, @"Blood\s+type:\s*(?<blood>[^\r\n]+)", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups["blood"].Value.Trim() : null;
        }

        static bool _IsTitleMatch(JikanAnimeSearchResult result, IEnumerable<string> searchTitles)
        {
            var normalizedSearchTitles = searchTitles
                .Select(_NormalizeTitle)
                .Where(title => !string.IsNullOrWhiteSpace(title))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var candidateTitles = new[]
            {
                result.Title,
                result.TitleEnglish,
                result.TitleJapanese
            }.Concat(result.TitleSynonyms ?? Array.Empty<string>());

            return candidateTitles
                .Select(_NormalizeTitle)
                .Any(title => !string.IsNullOrWhiteSpace(title) && normalizedSearchTitles.Contains(title));
        }

        static string _NormalizeTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            return Regex.Replace(title, @"[^\p{L}\p{N}]+", " ").Trim();
        }

        class JikanDataResponse<T>
        {
            [JsonPropertyName("data")]
            public T Data { get; set; }
        }

        class JikanAnimeCharacterEntry
        {
            [JsonPropertyName("character")]
            public JikanResource Character { get; set; }

            [JsonPropertyName("role")]
            public string Role { get; set; }

            [JsonPropertyName("voice_actors")]
            public List<JikanVoiceActor> VoiceActors { get; set; }
        }

        class JikanVoiceActor
        {
            [JsonPropertyName("person")]
            public JikanResource Person { get; set; }

            [JsonPropertyName("language")]
            public string Language { get; set; }
        }

        class JikanPerson
        {
            [JsonPropertyName("mal_id")]
            public int MalId { get; set; }

            [JsonPropertyName("url")]
            public string Url { get; set; }

            [JsonPropertyName("images")]
            public JikanImages Images { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("about")]
            public string About { get; set; }

            [JsonPropertyName("birthday")]
            public string Birthday { get; set; }

            [JsonPropertyName("voices")]
            public List<JikanPersonVoice> Voices { get; set; }
        }

        class JikanPersonVoice
        {
            [JsonPropertyName("role")]
            public string Role { get; set; }

            [JsonPropertyName("anime")]
            public JikanResource Anime { get; set; }

            [JsonPropertyName("character")]
            public JikanResource Character { get; set; }
        }

        class JikanAnimeSearchResult
        {
            [JsonPropertyName("mal_id")]
            public int MalId { get; set; }

            [JsonPropertyName("title")]
            public string Title { get; set; }

            [JsonPropertyName("title_english")]
            public string TitleEnglish { get; set; }

            [JsonPropertyName("title_japanese")]
            public string TitleJapanese { get; set; }

            [JsonPropertyName("title_synonyms")]
            public string[] TitleSynonyms { get; set; }
        }

        class JikanResource
        {
            [JsonPropertyName("mal_id")]
            public int MalId { get; set; }

            [JsonPropertyName("url")]
            public string Url { get; set; }

            [JsonPropertyName("images")]
            public JikanImages Images { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("title")]
            public string Title { get; set; }
        }

        class JikanImages
        {
            [JsonPropertyName("jpg")]
            public JikanJpgImage Jpg { get; set; }
        }

        class JikanJpgImage
        {
            [JsonPropertyName("image_url")]
            public string ImageUrl { get; set; }

            [JsonPropertyName("large_image_url")]
            public string LargeImageUrl { get; set; }
        }
    }
}
