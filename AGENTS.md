# AGENTS.md

This file provides guidance to agents such as OpenAI Codex when working with code in this repository.

## Project Overview

This is a Blazor WebAssembly application that helps anime fans discover shared voice actors (Seyu) across anime they've watched. Users can connect their Kitsu account to see characters voiced by the same actor in different anime.

**Live site**: https://www.animecharacters.app/

## Build and Development Commands

This is a .NET 8.0 solution with multiple projects:

```bash
# Build the entire solution
dotnet build

# Run the main Blazor WASM application locally
dotnet run --project AnimeCharacters/AnimeCharacters.csproj

# Run tests
dotnet test

# Build for release
dotnet build -c Release
```

The solution can be opened in Visual Studio 2022 or VS Code with the C# extension.

Note that merging to `main` will trigger a GitHub Action to build and deploy the application to production.

## Versioning

The application embeds a build-time version string in the format `yyyy-MM-bbbb` using the current UTC date and an incremental build number that resets each month.
The build number is zero-padded to four digits.
Debug builds prepend the version with `dev-`.

## Architecture

### Project Structure
- **AnimeCharacters/** - Main Blazor WebAssembly application
- **Kitsu/** - Client library for Kitsu.app API integration
- **AniListClient/** - GraphQL client for AniList API
- **ReferenceApis/** - Provider abstraction and implementations for character, anime reference, and seiyuu data
- **Kitsu.Tests/** - Unit tests for Kitsu client

### Key Components

#### Data Flow
1. Users authenticate with Kitsu.app to access their anime library
2. Library data is cached in browser LocalStorage via `DatabaseProvider`
3. Character/voice actor data is fetched through `ReferenceApis` providers
4. Enabled extensions contribute voice actor credits
5. Results are cross-referenced to find shared voice actors using provider-aware anime IDs

#### Main Services
- `DatabaseProvider` - LocalStorage wrapper with event notifications
- `PageStateManager` - Navigation state management
- `VoiceActorCreditService` - Aggregates enabled extension credits for voice actor pages
- `KitsuClient` - REST API client for Kitsu.app
- `AniListClient` - GraphQL client for AniList API; staff-history queries keep nested media data ID-only so large voice-actor histories remain responsive
- `ReferenceAnimeService` - Chooses reference API providers, merges provider IDs for matching, and falls back to an exact staff-name match when the routed provider is unavailable

#### Extensions
- **Kitsu Library** is the core anime extension and remains enabled.
- **Genshin Impact** is the first Video Games extension and is opt-in by default. Runtime reads checked-in JSON from `AnimeCharacters/wwwroot/data/extensions/genshin-impact-characters.json` and checked-in artwork from `AnimeCharacters/wwwroot/images/extensions/genshin-impact`.
- Refresh Genshin data with `.\tools\Update-GenshinImpactData.ps1` on Windows or `./tools/update-genshin-impact-data.sh` on macOS/Linux. Both scripts read Fandom's `Character/List` icon data, split multi-voice rows into separate character credits, cache poster/character images locally, and rate-limit API calls; pass `-ResolveAniListIds` or `--resolve-anilist-ids` only when you intentionally want the slower AniList staff ID enrichment.

#### Page Architecture
- All pages inherit from `BasePage` which provides common dependencies
- Pages use dependency injection for database, navigation, and event services
- Event aggregator pattern for inter-component communication

### External APIs
- **Kitsu.app API** - User authentication and anime library data
- **Jikan/MyAnimeList API** - Primary source for character and Japanese voice actor information
- **AniList GraphQL API** - Fallback source for character and voice actor information

## Development Notes

The application uses:
- MatBlazor for UI components
- Blazored.LocalStorage for client-side persistence
- EventAggregator.Blazor for pub/sub messaging
- Polly for retry policies
- Service worker for PWA capabilities

When working with API clients, note that Kitsu library data is separate from reference data. Add new character/anime reference providers behind `IReferenceAnimeProvider` and keep route IDs provider-aware so MAL and AniList IDs do not collide.

When changing or adding features, ensure to update the README.md, CLAUDE.md, and AGENTS.md files to keep documentation consistent.

## Code Styles
- Follow C# naming conventions and existing styles (PascalCase for public members, camelCase for private, private fields starting with `_`)
- Do not add excessive comments; code should be self-documenting with clear method names and comments to explain complex logic
- Use XML documentation comments for public APIs and methods
- Keep methods short and focused on a single responsibility
- Ensure code is extensible and maintainable, following SOLID principles
- This is mostly developed on a Windows machine, so ensure compatibility with Windows paths and tools, and use CLRF line endings instead of LF.
