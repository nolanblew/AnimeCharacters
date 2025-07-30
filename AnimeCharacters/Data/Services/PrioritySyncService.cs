using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AnimeCharacters.Data.Models;
using AniListClient.Models;

namespace AnimeCharacters.Data.Services
{
    public class PrioritySyncService : IPrioritySyncService, IDisposable
    {
        private readonly IDbContextFactory<AnimeCharactersDbContext> _contextFactory;
        private readonly AniListClient.AniListClient _aniListClient;
        private readonly ICharacterCacheService _characterCacheService;
        private readonly RateLimiter _rateLimiter;
        
        private readonly ConcurrentDictionary<string, PrioritySyncRequest> _pendingRequests = new();
        private readonly SemaphoreSlim _processingLock = new(1, 1);
        private CancellationTokenSource _backgroundCancellation;
        private Task _backgroundTask;
        private bool _isDisposed;

        private const int MAX_RETRY_COUNT = 3;
        private const int BACKGROUND_PROCESSING_DELAY_MS = 2000;
        private const int MAX_CONSECUTIVE_ERRORS = 10;

        public PrioritySyncService(
            IDbContextFactory<AnimeCharactersDbContext> contextFactory,
            AniListClient.AniListClient aniListClient,
            ICharacterCacheService characterCacheService)
        {
            _contextFactory = contextFactory;
            _aniListClient = aniListClient;
            _characterCacheService = characterCacheService;
            
            // Configure rate limiter: 60 requests per minute with exponential backoff
            _rateLimiter = new RateLimiter(
                maxRequests: 60,
                timeWindow: TimeSpan.FromMinutes(1),
                baseBackoffDelay: TimeSpan.FromSeconds(2)
            );
        }

        public async Task StartPrioritySyncAsync(CancellationToken cancellationToken = default)
        {
            if (_backgroundTask != null && !_backgroundTask.IsCompleted)
                return;

            _backgroundCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _backgroundTask = ProcessPriorityQueueAsync(_backgroundCancellation.Token);
            
            // Resume any interrupted sync state
            await ResumePendingSyncStateAsync();
        }

        public async Task StopPrioritySyncAsync()
        {
            _backgroundCancellation?.Cancel();
            if (_backgroundTask != null)
            {
                try
                {
                    await _backgroundTask;
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancelling
                }
            }
        }

        public async Task<T> RequestPrioritySyncAsync<T>(PrioritySyncRequest request, CancellationToken cancellationToken = default)
        {
            request.CancellationToken = cancellationToken;
            request.CompletionSource = new TaskCompletionSource<object>();

            // If a similar request is already pending, return that instead
            var existingRequest = _pendingRequests.Values
                .Where(r => r.Type == request.Type && r.ResourceId == request.ResourceId)
                .OrderByDescending(r => r.Priority)
                .FirstOrDefault();

            if (existingRequest != null)
            {
                // Upgrade priority if this request is higher
                if (request.Priority > existingRequest.Priority)
                {
                    existingRequest.Priority = request.Priority;
                }
                
                var result = await existingRequest.CompletionSource.Task;
                return (T)result;
            }

            _pendingRequests[request.Id] = request;

            try
            {
                var result = await request.CompletionSource.Task;
                return (T)result;
            }
            finally
            {
                _pendingRequests.TryRemove(request.Id, out _);
            }
        }

        public async Task<PrioritySyncStatus> GetPrioritySyncStatusAsync()
        {
            var rateLimitStatus = _rateLimiter.GetStatus();
            
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var todayStart = DateTime.UtcNow.Date;
            var processedToday = await GetDailyProcessedCountAsync(context, todayStart);
            var failedToday = await GetDailyFailedCountAsync(context, todayStart);

            return new PrioritySyncStatus
            {
                IsRunning = _backgroundTask != null && !_backgroundTask.IsCompleted,
                QueueSize = _pendingRequests.Count,
                ProcessedToday = processedToday,
                LastProcessedAt = await GetLastProcessedTimeAsync(context),
                FailedRequestsToday = failedToday,
                RateLimitStatus = rateLimitStatus
            };
        }

        public async Task ClearPrioritySyncQueueAsync()
        {
            // Cancel all pending requests
            foreach (var request in _pendingRequests.Values)
            {
                request.CompletionSource?.TrySetCanceled();
            }
            _pendingRequests.Clear();

            // Clear persisted queue state
            using var context = await _contextFactory.CreateDbContextAsync();
            await SetSyncMetadataAsync(context, SyncMetadataKeys.PrioritySyncQueueState, "");
            await context.SaveChangesAsync();
        }

        private async Task ProcessPriorityQueueAsync(CancellationToken cancellationToken)
        {
            var consecutiveErrors = 0;
            
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessNextRequestAsync(cancellationToken);
                    consecutiveErrors = 0; // Reset on success
                    await Task.Delay(BACKGROUND_PROCESSING_DELAY_MS, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    consecutiveErrors++;
                    Console.WriteLine($"Error in priority sync background processing ({consecutiveErrors}/{MAX_CONSECUTIVE_ERRORS}): {ex.Message}");
                    
                    if (consecutiveErrors >= MAX_CONSECUTIVE_ERRORS)
                    {
                        Console.WriteLine("Too many consecutive errors in background sync. Stopping.");
                        await StopPrioritySyncAsync();
                        break;
                    }
                    
                    // Exponential backoff for errors
                    var delay = TimeSpan.FromSeconds(Math.Min(10 * Math.Pow(2, consecutiveErrors - 1), 60));
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }

        private async Task ProcessNextRequestAsync(CancellationToken cancellationToken)
        {
            if (!_pendingRequests.Any())
                return;

            // Get highest priority request
            var request = _pendingRequests.Values
                .OrderByDescending(r => r.Priority)
                .ThenBy(r => r.RequestedAt)
                .FirstOrDefault();

            if (request == null)
                return;

            // Wait for rate limit availability
            await _rateLimiter.WaitForAvailabilityAsync(cancellationToken);

            try
            {
                var result = await ProcessSyncRequestAsync(request, cancellationToken);
                _rateLimiter.RecordSuccess();
                request.CompletionSource?.TrySetResult(result);
                
                await RecordSuccessAsync(request);
            }
            catch (Exception ex)
            {
                request.RetryCount++;
                request.LastAttemptAt = DateTime.UtcNow;

                // Check for various error types
                var isRateLimit = ex.Message.Contains("rate", StringComparison.OrdinalIgnoreCase) ||
                                 ex.Message.Contains("429", StringComparison.OrdinalIgnoreCase);
                
                var isCorsError = ex.Message.Contains("cors", StringComparison.OrdinalIgnoreCase) ||
                                 ex.Message.Contains("Access-Control-Allow-Origin", StringComparison.OrdinalIgnoreCase) ||
                                 ex.Message.Contains("preflight", StringComparison.OrdinalIgnoreCase);
                
                _rateLimiter.RecordFailure(isRateLimit, isCorsError);

                if (request.RetryCount >= MAX_RETRY_COUNT)
                {
                    request.CompletionSource?.TrySetException(ex);
                    await RecordFailureAsync(request, ex);
                }
                else
                {
                    // Keep request in queue for retry
                    return;
                }
            }

            _pendingRequests.TryRemove(request.Id, out _);
        }

        private async Task<object> ProcessSyncRequestAsync(PrioritySyncRequest request, CancellationToken cancellationToken)
        {
            switch (request.Type)
            {
                case PrioritySyncType.AnimeCharacters:
                    using (var context = await _contextFactory.CreateDbContextAsync())
                    {
                        var anime = await context.Anime
                            .FirstOrDefaultAsync(a => a.KitsuId == request.ResourceId, cancellationToken);
                        
                        if (anime != null && !string.IsNullOrEmpty(anime.AnilistId) && 
                            int.TryParse(anime.AnilistId, out var anilistId))
                        {
                            return await _characterCacheService.GetCharactersForAnimeAsync(
                                request.ResourceId, anime.AnilistId);
                        }
                    }
                    return new List<Character>();

                case PrioritySyncType.VoiceActorDetails:
                    if (int.TryParse(request.ResourceId, out var voiceActorId))
                    {
                        return await _characterCacheService.GetVoiceActorAsync(voiceActorId);
                    }
                    return null;

                case PrioritySyncType.CharacterDetails:
                    if (int.TryParse(request.ResourceId, out var characterId))
                    {
                        return await _characterCacheService.GetCharacterAsync(characterId);
                    }
                    return null;

                default:
                    throw new ArgumentException($"Unknown sync type: {request.Type}");
            }
        }

        private async Task ResumePendingSyncStateAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var queueStateJson = await GetSyncMetadataAsync(context, SyncMetadataKeys.PrioritySyncQueueState);
            if (string.IsNullOrEmpty(queueStateJson))
                return;

            try
            {
                var queueState = JsonSerializer.Deserialize<SyncQueueState>(queueStateJson);
                if (queueState?.PendingAnimeIds?.Any() == true)
                {
                    // Add pending anime to priority queue with low priority for background processing
                    foreach (var animeId in queueState.PendingAnimeIds)
                    {
                        var request = new PrioritySyncRequest
                        {
                            Type = PrioritySyncType.AnimeCharacters,
                            ResourceId = animeId,
                            Priority = PrioritySyncPriority.Low,
                            CompletionSource = new TaskCompletionSource<object>()
                        };
                        
                        _pendingRequests[request.Id] = request;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to resume sync state: {ex.Message}");
            }
        }

        #region Metadata and Statistics

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

        private async Task<int> GetDailyProcessedCountAsync(AnimeCharactersDbContext context, DateTime todayStart)
        {
            // This would need a separate table to track processing statistics
            // For now, return 0 as a placeholder
            return 0;
        }

        private async Task<int> GetDailyFailedCountAsync(AnimeCharactersDbContext context, DateTime todayStart)
        {
            // This would need a separate table to track processing statistics
            // For now, return 0 as a placeholder
            return 0;
        }

        private async Task<DateTime?> GetLastProcessedTimeAsync(AnimeCharactersDbContext context)
        {
            var lastProcessed = await GetSyncMetadataAsync(context, "LastPriorityProcessed");
            return DateTime.TryParse(lastProcessed, out var date) ? date : null;
        }

        private async Task RecordSuccessAsync(PrioritySyncRequest request)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            await SetSyncMetadataAsync(context, "LastPriorityProcessed", DateTime.UtcNow.ToString("O"));
            await context.SaveChangesAsync();
        }

        private async Task RecordFailureAsync(PrioritySyncRequest request, Exception ex)
        {
            // Log failure - could be expanded to store detailed failure information
            Console.WriteLine($"Priority sync failed for {request.Type}:{request.ResourceId} - {ex.Message}");
        }

        #endregion

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _backgroundCancellation?.Cancel();
            _backgroundTask?.Wait(TimeSpan.FromSeconds(5));
            
            _backgroundCancellation?.Dispose();
            _processingLock?.Dispose();
            _rateLimiter?.Dispose();
            
            _isDisposed = true;
        }
    }
}