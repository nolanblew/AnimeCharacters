using AnimeCharacters.Models;
using AnimeCharacters.Services;
using Kitsu;
using Kitsu.Controllers;
using Kitsu.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeCharacters.Services.Providers
{
    /// <summary>
    /// Kitsu.app provider for anime library data
    /// </summary>
    public class KitsuLibraryProvider : IAnimeLibraryProvider, IAnimeMetadataProvider
    {
        private readonly KitsuClient _kitsuClient;

        public KitsuLibraryProvider()
        {
            _kitsuClient = new KitsuClient();
        }

        public string ProviderId => "kitsu";
        public string ProviderName => "Kitsu.app";
        public int Priority => 100; // High priority as it's the primary library source
        public bool IsEnabled => true;

        public async Task<UnifiedUser> AuthenticateAsync(string username, string password)
        {
            // Note: The current KitsuClient doesn't seem to have authentication implemented
            // This would need to be implemented based on the existing auth flow
            throw new NotImplementedException("Kitsu authentication needs to be implemented");
        }

        public async Task<UnifiedUser> GetCurrentUserAsync()
        {
            // This would need to be implemented based on existing user storage
            throw new NotImplementedException("Get current user needs to be implemented");
        }

        public async Task<List<UnifiedLibraryEntry>> GetUserLibraryAsync(string userId)
        {
            var libraries = await _kitsuClient.UserLibraries.GetCompleteLibraryCollectionAsync(
                int.Parse(userId),
                LibraryType.Anime,
                Kitsu.Controllers.LibraryStatus.Current | Kitsu.Controllers.LibraryStatus.Completed);

            return libraries.Select(ConvertToUnifiedLibraryEntry).ToList();
        }

        public async Task<List<UnifiedLibraryEntry>> GetLibraryUpdatesAsync(string userId, object lastFetchToken)
        {
            if (lastFetchToken is not long lastFetchId)
                throw new ArgumentException("Invalid last fetch token for Kitsu provider");

            var deltaLibraryEvents = await _kitsuClient.UserLibraryEvents.GetLibraryEventsSinceTime(
                int.Parse(userId), lastFetchId);

            // Convert library events to unified entries
            // This is a simplified version - the actual implementation would need to handle
            // the complex delta logic from the original Animes.razor.cs
            var updatedEntries = new List<UnifiedLibraryEntry>();

            foreach (var libraryEvent in deltaLibraryEvents.LibraryEntryEvents)
            {
                if (libraryEvent.LibraryEntryId.HasValue)
                {
                    try
                    {
                        var entries = await _kitsuClient.UserLibraries.GetLibraryCollectionByIdsAsync(
                            int.Parse(userId),
                            new[] { libraryEvent.LibraryEntryId.Value },
                            LibraryType.Anime,
                            Kitsu.Controllers.LibraryStatus.Current | Kitsu.Controllers.LibraryStatus.Completed);

                        updatedEntries.AddRange(entries.Select(ConvertToUnifiedLibraryEntry));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error fetching library entry {libraryEvent.LibraryEntryId}: {ex.Message}");
                    }
                }
            }

            return updatedEntries;
        }

        public async Task<UnifiedAnime> GetAnimeAsync(string animeId)
        {
            // This would need to be implemented using the Kitsu API
            // Currently the KitsuClient doesn't expose anime details directly
            throw new NotImplementedException("Get anime details needs to be implemented");
        }

        public async Task<List<UnifiedAnime>> SearchAnimeAsync(string title)
        {
            // This would need to be implemented using the Kitsu search API
            throw new NotImplementedException("Search anime needs to be implemented");
        }

        private UnifiedLibraryEntry ConvertToUnifiedLibraryEntry(LibraryEntry kitsuEntry)
        {
            return new UnifiedLibraryEntry
            {
                Id = kitsuEntry.Id.ToString(),
                Anime = ConvertToUnifiedAnime(kitsuEntry.Anime),
                Status = ConvertLibraryStatus(kitsuEntry.Status),
                Progress = (int)(kitsuEntry.Progress ?? 0),
                ProgressedAt = kitsuEntry.ProgressedAt,
                StartedAt = kitsuEntry.StartedAt,
                FinishedAt = kitsuEntry.FinishedAt,
                IsReconsuming = kitsuEntry.IsReconsuming,
                ProviderId = ProviderId,
                OriginalData = kitsuEntry
            };
        }

        private UnifiedAnime ConvertToUnifiedAnime(Anime kitsuAnime)
        {
            var unified = new UnifiedAnime
            {
                Id = kitsuAnime.KitsuId,
                Title = kitsuAnime.Title,
                RomanjiTitle = kitsuAnime.RomanjiTitle,
                EnglishTitle = kitsuAnime.EnglishTitle,
                PosterImageUrl = kitsuAnime.PosterImageUrl,
                Type = ConvertAnimeType(kitsuAnime.ShowType),
                ProviderId = ProviderId,
                OriginalData = kitsuAnime
            };

            // Add provider IDs
            unified.ProviderIds[ProviderId] = kitsuAnime.KitsuId;
            if (!string.IsNullOrEmpty(kitsuAnime.AnilistId))
                unified.ProviderIds["anilist"] = kitsuAnime.AnilistId;
            if (!string.IsNullOrEmpty(kitsuAnime.MyAnimeListId))
                unified.ProviderIds["mal"] = kitsuAnime.MyAnimeListId;

            return unified;
        }

        private Models.LibraryStatus ConvertLibraryStatus(Kitsu.Controllers.LibraryStatus kitsuStatus)
        {
            return kitsuStatus switch
            {
                Kitsu.Controllers.LibraryStatus.Current => Models.LibraryStatus.Current,
                Kitsu.Controllers.LibraryStatus.Completed => Models.LibraryStatus.Completed,
                Kitsu.Controllers.LibraryStatus.OnHold => Models.LibraryStatus.OnHold,
                _ => Models.LibraryStatus.Current
            };
        }

        private Models.AnimeType? ConvertAnimeType(Kitsu.Models.AnimeType? kitsuType)
        {
            return kitsuType switch
            {
                Kitsu.Models.AnimeType.Show => Models.AnimeType.Show,
                Kitsu.Models.AnimeType.Movie => Models.AnimeType.Movie,
                Kitsu.Models.AnimeType.OVA => Models.AnimeType.OVA,
                _ => null
            };
        }
    }
}