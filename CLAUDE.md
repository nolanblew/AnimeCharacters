# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

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

## Architecture

### Project Structure
- **AnimeCharacters/** - Main Blazor WebAssembly application
- **Kitsu/** - Client library for Kitsu.app API integration
- **AniListClient/** - GraphQL client for AniList API
- **Kitsu.Tests/** - Unit tests for Kitsu client

### Key Components

#### Data Flow
1. Users authenticate with Kitsu.app to access their anime library
2. Library data is cached in browser LocalStorage via `DatabaseProvider`
3. Character/voice actor data is fetched from AniList GraphQL API
4. Results are cross-referenced to find shared voice actors

#### Main Services
- `DatabaseProvider` - LocalStorage wrapper with event notifications
- `PageStateManager` - Navigation state management
- `KitsuClient` - REST API client for Kitsu.app
- `AniListClient` - GraphQL client for AniList API

#### Page Architecture
- All pages inherit from `BasePage` which provides common dependencies
- Pages use dependency injection for database, navigation, and event services
- Event aggregator pattern for inter-component communication

### External APIs
- **Kitsu.app API** - User authentication and anime library data
- **AniList GraphQL API** - Character and voice actor information

## Development Notes

The application uses:
- MatBlazor for UI components
- Blazored.LocalStorage for client-side persistence
- EventAggregator.Blazor for pub/sub messaging
- Polly for retry policies
- Service worker for PWA capabilities

When working with API clients, note that both Kitsu and AniList have different response structures and require separate model classes in their respective projects.