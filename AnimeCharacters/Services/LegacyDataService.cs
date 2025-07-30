using AnimeCharacters.Models;
using Kitsu;
using Kitsu.Controllers;
using Kitsu.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeCharacters.Services
{
    /// <summary>
    /// Legacy compatibility service that provides the same interface as before
    /// while using the new provider system underneath
    /// </summary>
    public interface ILegacyDataService
    {
        /// <summary>
        /// Gets the user's anime library (compatible with existing code)
        /// </summary>
        Task<List<LibraryEntry>> GetUserLibraryAsync(long userId);

        /// <summary>
        /// Gets library updates since last fetch (compatible with existing code)
        /// </summary>
        Task<List<LibraryEntry>> GetLibraryUpdatesAsync(long userId, long lastFetchId);

        /// <summary>
        /// Gets characters for an anime using AniList
        /// </summary>
        Task<List<UnifiedCharacter>> GetCharactersForAnimeAsync(string anilistId);

        /// <summary>
        /// Gets voice actor information
        /// </summary>
        Task<UnifiedVoiceActor> GetVoiceActorAsync(int anilistId);

        /// <summary>
        /// Gets the latest library event ID for syncing
        /// </summary>
        Task<long> GetLatestLibraryEventIdAsync(long userId);
    }

    public class LegacyDataService : ILegacyDataService
    {
        private readonly IDataProviderService _dataProviderService;
        private readonly KitsuClient _kitsuClient; // Keep for methods not yet migrated

        public LegacyDataService(IDataProviderService dataProviderService)
        {
            _dataProviderService = dataProviderService;
            _kitsuClient = new KitsuClient(); // Fallback for non-migrated methods
        }

        public async Task<List<LibraryEntry>> GetUserLibraryAsync(long userId)
        {
            try
            {
                // Use the new provider system
                var unifiedEntries = await _dataProviderService.GetMergedUserLibraryAsync(userId.ToString());
                return unifiedEntries.Select(ConvertToKitsuLibraryEntry).ToList();
            }
            catch (Exception)
            {
                // Fallback to original implementation
                return await _kitsuClient.UserLibraries.GetCompleteLibraryCollectionAsync(
                    (int)userId, LibraryType.Anime, 
                    Kitsu.Controllers.LibraryStatus.Current | Kitsu.Controllers.LibraryStatus.Completed);
            }
        }

        public async Task<List<LibraryEntry>> GetLibraryUpdatesAsync(long userId, long lastFetchId)
        {
            // For now, fall back to original implementation since this is complex delta logic
            // Future enhancement: implement using provider system
            var deltaLibraryEvents = await _kitsuClient.UserLibraryEvents.GetLibraryEventsSinceTime((int)userId, lastFetchId);
            
            var updatedEntries = new List<LibraryEntry>();
            var idsToFetch = deltaLibraryEvents.LibraryEntryEvents
                .Where(e => e.LibraryEntryId.HasValue)
                .Select(e => e.LibraryEntryId.Value)
                .ToArray();

            if (idsToFetch.Any())
            {
                var entries = await _kitsuClient.UserLibraries.GetLibraryCollectionByIdsAsync(
                    (int)userId, idsToFetch, LibraryType.Anime,
                    Kitsu.Controllers.LibraryStatus.Current | Kitsu.Controllers.LibraryStatus.Completed);
                updatedEntries.AddRange(entries);
            }

            return updatedEntries;
        }

        public async Task<List<UnifiedCharacter>> GetCharactersForAnimeAsync(string anilistId)
        {
            var anime = new UnifiedAnime();
            anime.ProviderIds["anilist"] = anilistId;
            
            return await _dataProviderService.GetMergedCharactersForAnimeAsync(anime);
        }

        public async Task<UnifiedVoiceActor> GetVoiceActorAsync(int anilistId)
        {
            return await _dataProviderService.GetMergedVoiceActorAsync(anilistId.ToString(), "anilist");
        }

        public async Task<long> GetLatestLibraryEventIdAsync(long userId)
        {
            // Use original implementation
            var result = await _kitsuClient.UserLibraryEvents.GetLatestLibraryEventId((int)userId);
            return result ?? 0;
        }

        private LibraryEntry ConvertToKitsuLibraryEntry(UnifiedLibraryEntry unified)
        {
            // Convert unified entry back to Kitsu format for compatibility
            var kitsuAnime = new Anime(
                unified.Anime.ProviderIds.GetValueOrDefault("kitsu", unified.Anime.Id),
                unified.Anime.ProviderIds.GetValueOrDefault("mal", ""),
                unified.Anime.ProviderIds.GetValueOrDefault("anilist", ""),
                unified.Anime.Title,
                unified.Anime.RomanjiTitle,
                unified.Anime.EnglishTitle,
                "", // KitsuSlug - would need to be preserved
                unified.Anime.PosterImageUrl,
                "", // YoutubeId - would need to be preserved
                ConvertAnimeType(unified.Anime.Type)
            );

            return new LibraryEntry
            {
                Id = long.Parse(unified.Id),
                Anime = kitsuAnime,
                Status = ConvertLibraryStatus(unified.Status),
                Progress = unified.Progress,
                ProgressedAt = unified.ProgressedAt,
                StartedAt = unified.StartedAt,
                FinishedAt = unified.FinishedAt,
                IsReconsuming = unified.IsReconsuming
            };
        }

        private Kitsu.Controllers.LibraryStatus ConvertLibraryStatus(Models.LibraryStatus unifiedStatus)
        {
            return unifiedStatus switch
            {
                Models.LibraryStatus.Current => Kitsu.Controllers.LibraryStatus.Current,
                Models.LibraryStatus.Completed => Kitsu.Controllers.LibraryStatus.Completed,
                Models.LibraryStatus.OnHold => Kitsu.Controllers.LibraryStatus.OnHold,
                Models.LibraryStatus.Dropped => Kitsu.Controllers.LibraryStatus.OnHold, // Map to closest available
                Models.LibraryStatus.Planned => Kitsu.Controllers.LibraryStatus.Current, // Map to closest available
                _ => Kitsu.Controllers.LibraryStatus.Current
            };
        }

        private Kitsu.Models.AnimeType? ConvertAnimeType(Models.AnimeType? unifiedType)
        {
            return unifiedType switch
            {
                Models.AnimeType.Show => Kitsu.Models.AnimeType.Show,
                Models.AnimeType.Movie => Kitsu.Models.AnimeType.Movie,
                Models.AnimeType.OVA => Kitsu.Models.AnimeType.OVA,
                _ => null
            };
        }
    }
}