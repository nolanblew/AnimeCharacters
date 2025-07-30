using AnimeCharacters.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnimeCharacters.Services
{
    /// <summary>
    /// Base interface for all data providers that can supply anime-related information
    /// </summary>
    public interface IDataProvider
    {
        /// <summary>
        /// Unique identifier for this provider
        /// </summary>
        string ProviderId { get; }

        /// <summary>
        /// Display name for this provider
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Priority for this provider when merging data (higher = more trusted)
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Whether this provider is currently enabled/active
        /// </summary>
        bool IsEnabled { get; }
    }

    /// <summary>
    /// Interface for providers that can supply anime library information
    /// </summary>
    public interface IAnimeLibraryProvider : IDataProvider
    {
        /// <summary>
        /// Gets the user's anime library from this provider
        /// </summary>
        Task<List<UnifiedLibraryEntry>> GetUserLibraryAsync(string userId);

        /// <summary>
        /// Gets updates to the user's library since the last fetch
        /// </summary>
        Task<List<UnifiedLibraryEntry>> GetLibraryUpdatesAsync(string userId, object lastFetchToken);

        /// <summary>
        /// Authenticates the user with this provider
        /// </summary>
        Task<UnifiedUser> AuthenticateAsync(string username, string password);

        /// <summary>
        /// Gets the current authenticated user
        /// </summary>
        Task<UnifiedUser> GetCurrentUserAsync();
    }

    /// <summary>
    /// Interface for providers that can supply character and voice actor information
    /// </summary>
    public interface ICharacterDataProvider : IDataProvider
    {
        /// <summary>
        /// Gets character information for the specified anime
        /// </summary>
        Task<List<UnifiedCharacter>> GetCharactersForAnimeAsync(string animeId);

        /// <summary>
        /// Gets voice actor information by ID
        /// </summary>
        Task<UnifiedVoiceActor> GetVoiceActorAsync(string voiceActorId);

        /// <summary>
        /// Gets all characters voiced by a specific voice actor
        /// </summary>
        Task<List<UnifiedCharacter>> GetCharactersByVoiceActorAsync(string voiceActorId);
    }

    /// <summary>
    /// Interface for providers that can supply anime metadata
    /// </summary>
    public interface IAnimeMetadataProvider : IDataProvider
    {
        /// <summary>
        /// Gets detailed anime information by ID
        /// </summary>
        Task<UnifiedAnime> GetAnimeAsync(string animeId);

        /// <summary>
        /// Searches for anime by title
        /// </summary>
        Task<List<UnifiedAnime>> SearchAnimeAsync(string title);
    }
}