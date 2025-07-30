using AnimeCharacters.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnimeCharacters.Services
{
    /// <summary>
    /// Service that manages multiple data providers and handles data aggregation/merging
    /// </summary>
    public interface IDataProviderService
    {
        /// <summary>
        /// Registers a data provider
        /// </summary>
        void RegisterProvider<T>(T provider) where T : IDataProvider;

        /// <summary>
        /// Gets all registered providers of a specific type
        /// </summary>
        List<T> GetProviders<T>() where T : IDataProvider;

        /// <summary>
        /// Gets the primary (highest priority) provider of a specific type
        /// </summary>
        T GetPrimaryProvider<T>() where T : IDataProvider;

        /// <summary>
        /// Gets merged anime library data from all library providers
        /// </summary>
        Task<List<UnifiedLibraryEntry>> GetMergedUserLibraryAsync(string userId);

        /// <summary>
        /// Gets merged character data for an anime from all character providers
        /// </summary>
        Task<List<UnifiedCharacter>> GetMergedCharactersForAnimeAsync(UnifiedAnime anime);

        /// <summary>
        /// Gets merged voice actor data from all providers
        /// </summary>
        Task<UnifiedVoiceActor> GetMergedVoiceActorAsync(string voiceActorId, string providerId);

        /// <summary>
        /// Gets merged anime metadata from all providers
        /// </summary>
        Task<UnifiedAnime> GetMergedAnimeAsync(string animeId, string providerId);
    }
}