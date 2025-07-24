using AniListClient.Models;
using AnimeCharacters.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnimeCharacters.Data.Services
{
    public interface ICharacterCacheService
    {
        /// <summary>
        /// Gets characters for an anime, using cache first, then fetching from AniList if needed
        /// </summary>
        Task<List<Character>> GetCharactersForAnimeAsync(string kitsuId, string anilistId);
        
        /// <summary>
        /// Gets character details by character ID, using cache first
        /// </summary>
        Task<Character> GetCharacterAsync(int characterId);
        
        /// <summary>
        /// Gets voice actor details by voice actor ID, using cache first
        /// </summary>
        Task<Staff> GetVoiceActorAsync(int voiceActorId);
        
        /// <summary>
        /// Gets all anime that share characters with the given voice actor ID
        /// </summary>
        Task<List<CharacterAnimeModel>> GetSharedCharacterAnimesAsync(int voiceActorId, List<string> userAnimeKitsuIds);
        
        /// <summary>
        /// Pre-caches character data for all user's anime
        /// </summary>
        Task PreCacheUserAnimeCharactersAsync(List<string> animeKitsuIds);
        
        /// <summary>
        /// Clears all cached character data
        /// </summary>
        Task ClearCacheAsync();
        
        /// <summary>
        /// Gets cache statistics
        /// </summary>
        Task<CharacterCacheStats> GetCacheStatsAsync();
    }

    public class CharacterCacheStats
    {
        public int TotalCharacters { get; set; }
        public int TotalVoiceActors { get; set; }
        public int TotalAnimeCharacterRelations { get; set; }
        public int CachedAnimesCount { get; set; }
        public DateTime LastCacheUpdate { get; set; }
        public long CacheSizeBytes { get; set; }
    }
}