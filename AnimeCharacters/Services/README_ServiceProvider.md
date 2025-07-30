# Service Provider Architecture

This document describes the new service provider architecture implemented to address GitHub issue #32.

## Overview

The service provider architecture allows for multiple data sources to be integrated into the application, enabling future extensions for different anime databases, game databases, or other content providers.

## Architecture Components

### Core Interfaces

1. **IDataProvider** - Base interface for all providers
2. **IAnimeLibraryProvider** - For providers that supply user anime libraries
3. **ICharacterDataProvider** - For providers that supply character and voice actor data
4. **IAnimeMetadataProvider** - For providers that supply anime metadata

### Unified Models

- **UnifiedAnime** - Common anime representation across providers
- **UnifiedCharacter** - Common character representation
- **UnifiedVoiceActor** - Common voice actor representation
- **UnifiedLibraryEntry** - Common library entry representation
- **UnifiedUser** - Common user representation

### Service Layer

- **IDataProviderService** - Manages multiple providers and handles data merging
- **DataProviderService** - Default implementation with duplicate detection logic
- **ILegacyDataService** - Compatibility layer for existing code

## Current Providers

### KitsuLibraryProvider
- Provides anime library data from Kitsu.app
- Implements IAnimeLibraryProvider and IAnimeMetadataProvider
- Priority: 100 (primary library source)

### AniListCharacterProvider
- Provides character and voice actor data from AniList
- Implements ICharacterDataProvider
- Priority: 90 (high priority for character data)

## Data Merging Strategy

The system uses a multi-step approach to merge data from different providers:

1. **Provider ID Matching** - Links items using cross-provider IDs (e.g., Kitsu ID, AniList ID, MAL ID)
2. **Name-based Matching** - Falls back to normalized title/name matching when provider IDs aren't available
3. **Priority-based Selection** - Uses higher priority providers for primary data when conflicts occur

## Adding New Providers

To add a new provider:

1. Create a new class implementing the appropriate interface(s)
2. Convert external data models to unified models
3. Register the provider in Program.cs
4. Configure priority relative to existing providers

Example:
```csharp
public class MyAnimeListProvider : IAnimeLibraryProvider
{
    public string ProviderId => "mal";
    public string ProviderName => "MyAnimeList";
    public int Priority => 95;
    public bool IsEnabled => true;
    
    // Implement interface methods...
}
```

## Compatibility

The existing codebase continues to work unchanged through the LegacyDataService compatibility layer. This service:

- Provides the same interface as the original direct client usage
- Uses the new provider system underneath when possible
- Falls back to original implementations for complex operations

## Future Enhancements

1. **Configuration-based Provider Management** - Allow enabling/disabling providers via settings
2. **Enhanced Duplicate Detection** - More sophisticated matching algorithms
3. **Caching Layer** - Cache merged data to improve performance
4. **Plugin System** - Load providers dynamically from external assemblies
5. **Data Validation** - Validate merged data for consistency