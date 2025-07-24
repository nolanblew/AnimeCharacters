using AnimeCharacters.Data.Extensions;
using AnimeCharacters.Data.Models;
using AniListClient.Models;
using Kitsu;
using Kitsu.Controllers;
using Kitsu.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AnimeCharacters.Data.Services
{
    public class SyncService : ISyncService
    {
        private readonly IDbContextFactory<AnimeCharactersDbContext> _contextFactory;
        private readonly KitsuClient _kitsuClient;
        private readonly AniListClient.AniListClient _aniListClient;
        private readonly SemaphoreSlim _syncSemaphore = new(1, 1);
        private CancellationTokenSource _currentSyncCancellation;

        private const int CACHE_REFRESH_TIME_FORCE_REFRESH_DAYS = 6;
        private const int CACHE_UPDATE_TIME_SECONDS = 15;

        public SyncService(
            IDbContextFactory<AnimeCharactersDbContext> contextFactory,
            KitsuClient kitsuClient,
            AniListClient.AniListClient aniListClient)
        {
            _contextFactory = contextFactory;
            _kitsuClient = kitsuClient;
            _aniListClient = aniListClient;
        }

        public async Task<SyncResult> PerformFullSyncAsync(SyncProgressCallback progressCallback = null, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var statistics = new SyncStatistics();

            if (!await _syncSemaphore.WaitAsync(0, cancellationToken))
            {
                return new SyncResult { IsSuccess = false, ErrorMessage = "Sync already in progress" };
            }

            try
            {
                _currentSyncCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var token = _currentSyncCancellation.Token;

                await UpdateSyncStatusAsync(SyncStage.Initializing, 0, 100, "Initializing full sync...");
                await progressCallback?.Invoke(new SyncProgress
                {
                    Stage = SyncStage.Initializing,
                    StageName = "Initializing",
                    CurrentStep = 0,
                    TotalSteps = 100,
                    CurrentOperation = "Preparing for full synchronization...",
                    StartTime = DateTime.UtcNow
                });

                // Get current user
                var user = await GetCurrentUserAsync();
                if (user == null)
                {
                    return new SyncResult { IsSuccess = false, ErrorMessage = "No user found" };
                }

                // Stage 1: Fetch all library data from Kitsu
                await UpdateSyncStatusAsync(SyncStage.FetchingLibraries, 10, 100, "Fetching anime library...");
                await progressCallback?.Invoke(new SyncProgress
                {
                    Stage = SyncStage.FetchingLibraries,
                    StageName = "Fetching Libraries",
                    CurrentStep = 10,
                    TotalSteps = 100,
                    CurrentOperation = "Downloading anime library from Kitsu...",
                    StartTime = DateTime.UtcNow
                });

                var libraryEntries = await _kitsuClient.UserLibraries.GetCompleteLibraryCollectionAsync(
                    user.Id,
                    LibraryType.Anime,
                    LibraryStatus.Current | LibraryStatus.Completed);

                statistics.LibraryEntriesUpdated = libraryEntries.Count;
                token.ThrowIfCancellationRequested();

                // Stage 2: Update database with library data
                await UpdateSyncStatusAsync(SyncStage.UpdatingDatabase, 30, 100, "Updating library database...");
                await progressCallback?.Invoke(new SyncProgress
                {
                    Stage = SyncStage.UpdatingDatabase,
                    StageName = "Updating Database",
                    CurrentStep = 30,
                    TotalSteps = 100,
                    CurrentOperation = "Saving library data to database...",
                    StartTime = DateTime.UtcNow
                });

                await UpdateLibraryEntriesAsync(user.Id, libraryEntries);
                statistics.AnimeSynced = libraryEntries.Count;

                // Stage 3: Fetch character and voice actor data for anime
                await UpdateSyncStatusAsync(SyncStage.FetchingCharacters, 50, 100, "Fetching character data...");
                await progressCallback?.Invoke(new SyncProgress
                {
                    Stage = SyncStage.FetchingCharacters,
                    StageName = "Fetching Characters",
                    CurrentStep = 50,
                    TotalSteps = 100,
                    CurrentOperation = "Downloading character and voice actor data...",
                    StartTime = DateTime.UtcNow
                });

                var charactersUpdated = await FetchAndCacheCharacterDataAsync(libraryEntries, progressCallback, token);
                statistics.CharactersSynced = charactersUpdated.Characters;
                statistics.VoiceActorsSynced = charactersUpdated.VoiceActors;

                // Stage 4: Finalize
                await UpdateSyncStatusAsync(SyncStage.Finalizing, 90, 100, "Finalizing sync...");
                await progressCallback?.Invoke(new SyncProgress
                {
                    Stage = SyncStage.Finalizing,
                    StageName = "Finalizing",
                    CurrentStep = 90,
                    TotalSteps = 100,
                    CurrentOperation = "Completing synchronization...",
                    StartTime = DateTime.UtcNow
                });

                // Update sync metadata
                using var context = await _contextFactory.CreateDbContextAsync();
                await SetSyncMetadataAsync(context, SyncMetadataKeys.LastFullSyncDate, DateTime.UtcNow.ToString("O"));
                await SetSyncMetadataAsync(context, SyncMetadataKeys.LastFetchedDate, DateTimeOffset.UtcNow.ToString("O"));
                await SetSyncMetadataAsync(context, SyncMetadataKeys.SyncInProgress, "false");
                await context.SaveChangesAsync(token);

                await UpdateSyncStatusAsync(SyncStage.Completed, 100, 100, "Sync completed successfully");
                await progressCallback?.Invoke(new SyncProgress
                {
                    Stage = SyncStage.Completed,
                    StageName = "Completed",
                    CurrentStep = 100,
                    TotalSteps = 100,
                    CurrentOperation = "Synchronization completed successfully",
                    StartTime = DateTime.UtcNow
                });

                return new SyncResult
                {
                    IsSuccess = true,
                    Strategy = SyncStrategy.FullSync,
                    Duration = stopwatch.Elapsed,
                    Statistics = statistics
                };
            }
            catch (OperationCanceledException)
            {
                await UpdateSyncStatusAsync(SyncStage.Error, 0, 100, "Sync cancelled");
                return new SyncResult { IsSuccess = false, ErrorMessage = "Sync was cancelled" };
            }
            catch (Exception ex)
            {
                await UpdateSyncStatusAsync(SyncStage.Error, 0, 100, $"Sync failed: {ex.Message}");
                return new SyncResult { IsSuccess = false, ErrorMessage = ex.Message };
            }
            finally
            {
                _syncSemaphore.Release();
                _currentSyncCancellation?.Dispose();
                _currentSyncCancellation = null;
                stopwatch.Stop();
            }
        }

        public async Task<SyncResult> PerformDeltaSyncAsync(SyncProgressCallback progressCallback = null, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var statistics = new SyncStatistics();

            if (!await _syncSemaphore.WaitAsync(0, cancellationToken))
            {
                return new SyncResult { IsSuccess = false, ErrorMessage = "Sync already in progress" };
            }

            try
            {
                _currentSyncCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var token = _currentSyncCancellation.Token;

                await UpdateSyncStatusAsync(SyncStage.Initializing, 0, 100, "Initializing delta sync...");
                await progressCallback?.Invoke(new SyncProgress
                {
                    Stage = SyncStage.Initializing,
                    StageName = "Initializing",
                    CurrentStep = 0,
                    TotalSteps = 100,
                    CurrentOperation = "Preparing for delta synchronization...",
                    StartTime = DateTime.UtcNow
                });

                // Get current user and last fetched ID
                var user = await GetCurrentUserAsync();
                if (user == null)
                {
                    return new SyncResult { IsSuccess = false, ErrorMessage = "No user found" };
                }

                var lastFetchedId = await GetLastFetchedIdAsync();
                if (!lastFetchedId.HasValue)
                {
                    // Fall back to full sync if no baseline
                    return await PerformFullSyncAsync(progressCallback, cancellationToken);
                }

                // Fetch delta events
                await UpdateSyncStatusAsync(SyncStage.FetchingLibraries, 20, 100, "Fetching library updates...");
                await progressCallback?.Invoke(new SyncProgress
                {
                    Stage = SyncStage.FetchingLibraries,
                    StageName = "Fetching Updates",
                    CurrentStep = 20,
                    TotalSteps = 100,
                    CurrentOperation = "Downloading recent changes...",
                    StartTime = DateTime.UtcNow
                });

                var deltaEvents = await _kitsuClient.UserLibraryEvents.GetLibraryEventsSinceTime(user.Id, lastFetchedId.Value);
                token.ThrowIfCancellationRequested();

                // Process delta changes
                await UpdateSyncStatusAsync(SyncStage.UpdatingDatabase, 50, 100, "Applying updates...");
                await progressCallback?.Invoke(new SyncProgress
                {
                    Stage = SyncStage.UpdatingDatabase,
                    StageName = "Applying Updates",
                    CurrentStep = 50,
                    TotalSteps = 100,
                    CurrentOperation = "Updating database with changes...",
                    StartTime = DateTime.UtcNow
                });

                var updateResult = await ProcessDeltaEventsAsync(user.Id, deltaEvents.LibraryEntryEvents, token);
                statistics.LibraryEntriesUpdated = updateResult.UpdatedEntries;
                statistics.NewItemsAdded = updateResult.NewEntries;

                // Update sync metadata
                await UpdateSyncStatusAsync(SyncStage.Finalizing, 90, 100, "Finalizing...");
                using var context = await _contextFactory.CreateDbContextAsync();
                await SetSyncMetadataAsync(context, SyncMetadataKeys.LastFetchedDate, DateTimeOffset.UtcNow.ToString("O"));
                if (deltaEvents.LastFetchedId > 0)
                {
                    await SetSyncMetadataAsync(context, SyncMetadataKeys.LastFetchedId, deltaEvents.LastFetchedId.ToString());
                }
                await SetSyncMetadataAsync(context, SyncMetadataKeys.SyncInProgress, "false");
                await context.SaveChangesAsync(token);

                await UpdateSyncStatusAsync(SyncStage.Completed, 100, 100, "Delta sync completed");
                await progressCallback?.Invoke(new SyncProgress
                {
                    Stage = SyncStage.Completed,
                    StageName = "Completed",
                    CurrentStep = 100,
                    TotalSteps = 100,
                    CurrentOperation = "Delta synchronization completed",
                    StartTime = DateTime.UtcNow
                });

                return new SyncResult
                {
                    IsSuccess = true,
                    Strategy = SyncStrategy.DeltaSync,
                    Duration = stopwatch.Elapsed,
                    Statistics = statistics
                };
            }
            catch (OperationCanceledException)
            {
                await UpdateSyncStatusAsync(SyncStage.Error, 0, 100, "Sync cancelled");
                return new SyncResult { IsSuccess = false, ErrorMessage = "Delta sync was cancelled" };
            }
            catch (Exception ex)
            {
                await UpdateSyncStatusAsync(SyncStage.Error, 0, 100, $"Delta sync failed: {ex.Message}");
                return new SyncResult { IsSuccess = false, ErrorMessage = ex.Message };
            }
            finally
            {
                _syncSemaphore.Release();
                _currentSyncCancellation?.Dispose();
                _currentSyncCancellation = null;
                stopwatch.Stop();
            }
        }

        public async Task<SyncStatus> GetSyncStatusAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var isSyncInProgress = await GetSyncMetadataAsync(context, SyncMetadataKeys.SyncInProgress) == "true";
            var currentStageStr = await GetSyncMetadataAsync(context, SyncMetadataKeys.SyncProgressStage);
            var progressStr = await GetSyncMetadataAsync(context, SyncMetadataKeys.SyncProgressPercent);
            var lastSyncStr = await GetSyncMetadataAsync(context, SyncMetadataKeys.LastFetchedDate);
            var lastFullSyncStr = await GetSyncMetadataAsync(context, SyncMetadataKeys.LastFullSyncDate);

            Enum.TryParse<SyncStage>(currentStageStr, out var currentStage);
            double.TryParse(progressStr, out var progress);
            DateTimeOffset.TryParse(lastSyncStr, out var lastSync);
            DateTimeOffset.TryParse(lastFullSyncStr, out var lastFullSync);

            return new SyncStatus
            {
                IsSyncInProgress = isSyncInProgress,
                CurrentStage = isSyncInProgress ? currentStage : null,
                ProgressPercentage = progress,
                LastSyncDate = lastSync == default ? null : lastSync.DateTime,
                LastFullSyncDate = lastFullSync == default ? null : lastFullSync.DateTime,
                CanResume = isSyncInProgress // Can resume if sync is marked as in progress but not actually running
            };
        }

        public async Task CancelSyncAsync()
        {
            _currentSyncCancellation?.Cancel();
            using var context = await _contextFactory.CreateDbContextAsync();
            await SetSyncMetadataAsync(context, SyncMetadataKeys.SyncInProgress, "false");
            await context.SaveChangesAsync();
        }

        public async Task<SyncResult> ResumeSyncAsync(SyncProgressCallback progressCallback = null, CancellationToken cancellationToken = default)
        {
            var syncStatus = await GetSyncStatusAsync();
            if (!syncStatus.CanResume)
            {
                return new SyncResult { IsSuccess = false, ErrorMessage = "No sync to resume" };
            }

            // For simplicity, restart the sync process
            // In a more advanced implementation, you could resume from the exact stage
            var strategy = await DetermineSyncStrategyAsync();
            return strategy switch
            {
                SyncStrategy.FullSync => await PerformFullSyncAsync(progressCallback, cancellationToken),
                SyncStrategy.DeltaSync => await PerformDeltaSyncAsync(progressCallback, cancellationToken),
                _ => await PerformFullSyncAsync(progressCallback, cancellationToken)
            };
        }

        public async Task<SyncStrategy> DetermineSyncStrategyAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var lastFetchedDateStr = await GetSyncMetadataAsync(context, SyncMetadataKeys.LastFetchedDate);
            var lastFullSyncStr = await GetSyncMetadataAsync(context, SyncMetadataKeys.LastFullSyncDate);
            var libraryCount = await context.LibraryEntries.CountAsync();

            // If no data exists, do full sync
            if (libraryCount == 0 || string.IsNullOrEmpty(lastFetchedDateStr))
            {
                return SyncStrategy.FullSync;
            }

            // If last full sync was more than CACHE_REFRESH_TIME_FORCE_REFRESH_DAYS ago, do full sync
            if (DateTimeOffset.TryParse(lastFullSyncStr, out var lastFullSync) &&
                DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(CACHE_REFRESH_TIME_FORCE_REFRESH_DAYS)) > lastFullSync)
            {
                return SyncStrategy.FullSync;
            }

            // If last sync was more than CACHE_UPDATE_TIME_SECONDS ago, do delta sync
            if (DateTimeOffset.TryParse(lastFetchedDateStr, out var lastFetched) &&
                DateTimeOffset.UtcNow.Subtract(TimeSpan.FromSeconds(CACHE_UPDATE_TIME_SECONDS)) > lastFetched)
            {
                return SyncStrategy.DeltaSync;
            }

            // Data is recent enough, no sync needed
            return SyncStrategy.DeltaSync;
        }

        // Helper methods
        private async Task<User> GetCurrentUserAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var dbUser = await context.Users.FirstOrDefaultAsync();
            return dbUser?.ToUser();
        }

        private async Task<long?> GetLastFetchedIdAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var value = await GetSyncMetadataAsync(context, SyncMetadataKeys.LastFetchedId);
            return long.TryParse(value, out var id) ? id : null;
        }

        private async Task UpdateSyncStatusAsync(SyncStage stage, int currentStep, int totalSteps, string operation)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            await SetSyncMetadataAsync(context, SyncMetadataKeys.SyncInProgress, "true");
            await SetSyncMetadataAsync(context, SyncMetadataKeys.SyncProgressStage, stage.ToString());
            await SetSyncMetadataAsync(context, SyncMetadataKeys.SyncProgressPercent, ((double)currentStep / totalSteps * 100).ToString("F1"));
            await context.SaveChangesAsync();
        }

        private async Task<string> GetSyncMetadataAsync(AnimeCharactersDbContext context, string key)
        {
            var metadata = await context.SyncMetadata.FirstOrDefaultAsync(m => m.Key == key);
            return metadata?.Value;
        }

        private async Task SetSyncMetadataAsync(AnimeCharactersDbContext context, string key, string value)
        {
            var metadata = await context.SyncMetadata.FirstOrDefaultAsync(m => m.Key == key);
            if (metadata != null)
            {
                metadata.Value = value;
                metadata.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                context.SyncMetadata.Add(new DbSyncMetadata { Key = key, Value = value });
            }
        }

        private async Task UpdateLibraryEntriesAsync(int userId, List<LibraryEntry> libraryEntries)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Remove existing entries for this user
            var existingEntries = await context.LibraryEntries.Where(le => le.UserId == userId).ToListAsync();
            context.LibraryEntries.RemoveRange(existingEntries);

            // Add updated entries
            foreach (var entry in libraryEntries)
            {
                var dbEntry = entry.ToDbLibraryEntry();
                dbEntry.UserId = userId;

                // Ensure anime exists
                if (entry.Anime != null)
                {
                    var existingAnime = await context.Anime.FirstOrDefaultAsync(a => a.KitsuId == entry.Anime.KitsuId);
                    if (existingAnime == null)
                    {
                        context.Anime.Add(entry.Anime.ToDbAnime());
                    }
                    else
                    {
                        // Update existing anime
                        var updatedAnime = entry.Anime.ToDbAnime();
                        existingAnime.Title = updatedAnime.Title;
                        existingAnime.RomanjiTitle = updatedAnime.RomanjiTitle;
                        existingAnime.EnglishTitle = updatedAnime.EnglishTitle;
                        existingAnime.PosterImageUrl = updatedAnime.PosterImageUrl;
                        existingAnime.UpdatedAt = DateTime.UtcNow;
                    }
                }

                context.LibraryEntries.Add(dbEntry);
            }

            await context.SaveChangesAsync();
        }

        private async Task<(int Characters, int VoiceActors)> FetchAndCacheCharacterDataAsync(
            List<LibraryEntry> libraryEntries, 
            SyncProgressCallback progressCallback, 
            CancellationToken cancellationToken)
        {
            var charactersCount = 0;
            var voiceActorsCount = 0;
            var processedCount = 0;
            var totalAnime = libraryEntries.Count;

            foreach (var entry in libraryEntries)
            {
                if (cancellationToken.IsCancellationRequested) break;

                if (!string.IsNullOrEmpty(entry.Anime?.AnilistId))
                {
                    try
                    {
                        var anilistId = int.Parse(entry.Anime.AnilistId);
                        var media = await _aniListClient.Characters.GetMediaWithCharactersById(anilistId);
                        
                        if (media?.Characters != null)
                        {
                            await CacheCharacterDataAsync(entry.Anime.KitsuId, media.Characters);
                            charactersCount += media.Characters.Count;
                            voiceActorsCount += media.Characters.SelectMany(c => c.VoiceActors).Count();
                        }
                    }
                    catch
                    {
                        // Skip failed requests
                    }
                }

                processedCount++;
                var progress = 50 + (int)((double)processedCount / totalAnime * 40); // Characters stage is 50-90%
                
                await progressCallback?.Invoke(new SyncProgress
                {
                    Stage = SyncStage.FetchingCharacters,
                    StageName = "Fetching Characters",
                    CurrentStep = progress,
                    TotalSteps = 100,
                    CurrentOperation = $"Processing characters for {entry.Anime?.Title ?? "anime"} ({processedCount}/{totalAnime})",
                    StartTime = DateTime.UtcNow
                });
            }

            return (charactersCount, voiceActorsCount);
        }

        private async Task CacheCharacterDataAsync(string animeKitsuId, List<Character> characters)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            foreach (var character in characters)
            {
                // Cache character
                var existingChar = await context.Characters.FirstOrDefaultAsync(c => c.Id == character.Id);
                if (existingChar == null)
                {
                    context.Characters.Add(character.ToDbCharacter());
                }

                // Cache anime-character relationship
                var existingAnimeChar = await context.AnimeCharacters
                    .FirstOrDefaultAsync(ac => ac.AnimeKitsuId == animeKitsuId && ac.CharacterId == character.Id);
                if (existingAnimeChar == null)
                {
                    context.AnimeCharacters.Add(new DbAnimeCharacter
                    {
                        AnimeKitsuId = animeKitsuId,
                        CharacterId = character.Id,
                        Role = character.Role
                    });
                }

                // Cache voice actors
                foreach (var voiceActor in character.VoiceActors)
                {
                    var existingVA = await context.VoiceActors.FirstOrDefaultAsync(va => va.Id == voiceActor.Id);
                    if (existingVA == null)
                    {
                        context.VoiceActors.Add(new DbVoiceActor
                        {
                            Id = voiceActor.Id,
                            NameRomaji = voiceActor.Name.Romaji,
                            NameFirst = voiceActor.Name.First,
                            NameLast = voiceActor.Name.Last,
                            NameFull = voiceActor.Name.Full,
                            NameNative = voiceActor.Name.Native,
                            NameAlternative = voiceActor.Name.Alternative,
                            NameAlternativeSpoiler = voiceActor.Name.AlternativeSpoiler
                        });
                    }

                    // Cache character-voice actor relationship
                    var existingCharVA = await context.CharacterVoiceActors
                        .FirstOrDefaultAsync(cva => cva.CharacterId == character.Id && cva.VoiceActorId == voiceActor.Id);
                    if (existingCharVA == null)
                    {
                        context.CharacterVoiceActors.Add(new DbCharacterVoiceActor
                        {
                            CharacterId = character.Id,
                            VoiceActorId = voiceActor.Id
                        });
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        private async Task<(int UpdatedEntries, int NewEntries)> ProcessDeltaEventsAsync(
            int userId, 
            List<Kitsu.Models.LibraryEntryEvent> libraryEntryEvents, 
            CancellationToken cancellationToken)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var updatedCount = 0;
            var newCount = 0;

            foreach (var eventItem in libraryEntryEvents)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (eventItem.LibraryEntryId.HasValue)
                {
                    var existingEntry = await context.LibraryEntries
                        .FirstOrDefaultAsync(le => le.Id == eventItem.LibraryEntryId.Value);

                    if (existingEntry != null && eventItem.LibraryEntrySlim != null)
                    {
                        // Update existing entry
                        var slimEntry = eventItem.LibraryEntrySlim;
                        existingEntry.Status = slimEntry.Status;
                        existingEntry.Progress = slimEntry.Progress;
                        existingEntry.ProgressedAt = slimEntry.ProgressedAt;
                        existingEntry.IsReconsuming = slimEntry.IsReconsuming;
                        existingEntry.FinishedAt = slimEntry.FinishedAt;
                        existingEntry.UpdatedAt = DateTime.UtcNow;
                        updatedCount++;
                    }
                    else
                    {
                        // This is a new entry - we should fetch the full data
                        // For now, we'll create a placeholder and let the next full sync handle it
                        newCount++;
                    }
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            return (updatedCount, newCount);
        }
    }
}