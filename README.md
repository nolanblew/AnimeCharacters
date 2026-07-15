# Anime Characters
<img src="http://thankful-hill-03c7b7e1e.azurestaticapps.net/icon-512.png" width="90" height="90" />
Quickly find the characters that a Seyu (voice actor) has also played in other anime you've watched as listed on [Kitsu](https://kitsu.app)

The app supports optional media extensions. The core extension is **Kitsu Library** for anime, and **Genshin Impact** is available under the Video Games category so shared Japanese voice actors can appear alongside anime credits.

**Live site**: https://www.animecharacters.app/

## Requirements to Use
 - [Kitsu account](https://kitsu.app). If you know your username (slug) you don't have to log in. You may also login with your email address and password
 - Internet connection
 - Modern web browser (cross-platform).

 Note that the app can be installed as a Progressive Web App (PWA), though offline capabilities currently do not exist.

 ## Building Locally
 - **Framework**: Blazor Web Assembly (WASM) app
 - **IDE**: Visual Studio 2022 (though VSCode should work as well)
 - **Platform**: Windows/OSX/Linux
 - **.NET Framework**: 8.0

Not much else is needed, and you can build on any platform that supports dotnet.

## Versioning

The application generates a version at build time using the format `yyyy-MM-bbbb` (UTC date and an incremental build number that resets each month).
The build number is zero-padded to four digits.
Debug builds are prefixed with `dev-`.

## Reference Data Providers

Kitsu remains the source for user library data. Character, anime reference, and seiyuu data is loaded through the `ReferenceApis` project so providers can be swapped or combined without changing page logic. Jikan/MyAnimeList is currently registered first because it exposes character and Japanese voice actor data by MAL anime id, while AniList remains available as a fallback provider. Voice actor routes include the displayed staff name so a failed staff lookup can resolve an exact name match through another configured provider without changing the primary anime provider. AniList staff-history requests fetch only provider IDs and character display data; library titles remain the display source, which keeps large voice-actor histories responsive.

# Contributing
Contributions would be greatly appreciated. Feel free to create issues as well as forking the repo and creating PRs.
If you wish to make some major contributions please create an issue and we can evauate making you a contributer.

Note that this is a side-project for me so I may not immediately respond.

# Deploying
A deploy is automatically commenced when a pull request is merged to master. A deploy takes approximately 5 minutes, and can be viewed in the [Actions tab](https://github.com/nolanblew/AnimeCharacters/actions/workflows/azure-static-web-apps-thankful-hill-03c7b7e1e.yml).

## Extension Data

Genshin Impact character data is checked in under `AnimeCharacters/wwwroot/data/extensions/genshin-impact-characters.json` for low runtime overhead. Poster and character artwork is cached under `AnimeCharacters/wwwroot/images/extensions/genshin-impact` so the app does not hotlink Fandom images at runtime. Video game extensions are opt-in by default. The updater reads Fandom's `Character/List` page for character icon URLs and the `Voice_Actor` page for Japanese voice actor credits, splitting multi-voice rows into separate character credits such as Traveler (Male) and Traveler (Female). Refresh it on Windows with:

```powershell
.\tools\Update-GenshinImpactData.ps1
```

On macOS/Linux, use the bash wrapper:

```bash
./tools/update-genshin-impact-data.sh
```

Both scripts rate-limit API calls. Existing cached artwork is reused by default, but missing artwork for new characters is downloaded automatically; use `-RefreshImages` in PowerShell or `--refresh-images` in bash only when you intentionally want to replace already-cached images. Pass `-ResolveAniListIds` in PowerShell or `--resolve-anilist-ids` in bash only when intentionally enriching staff IDs through the slower AniList lookup path.
