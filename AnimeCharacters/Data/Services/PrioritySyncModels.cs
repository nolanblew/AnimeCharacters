using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AnimeCharacters.Data.Services
{
    public interface IPrioritySyncService
    {
        /// <summary>
        /// Starts the background priority sync queue processing
        /// </summary>
        Task StartPrioritySyncAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stops the background priority sync queue processing
        /// </summary>
        Task StopPrioritySyncAsync();

        /// <summary>
        /// Requests priority sync for specific anime characters/voice actors
        /// </summary>
        Task<T> RequestPrioritySyncAsync<T>(PrioritySyncRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the current state of the priority sync queue
        /// </summary>
        Task<PrioritySyncStatus> GetPrioritySyncStatusAsync();

        /// <summary>
        /// Clears all pending priority sync requests
        /// </summary>
        Task ClearPrioritySyncQueueAsync();
    }

    public class PrioritySyncRequest
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public PrioritySyncType Type { get; set; }
        public string ResourceId { get; set; } // AnimeKitsuId, CharacterId, VoiceActorId
        public PrioritySyncPriority Priority { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public int RetryCount { get; set; } = 0;
        public DateTime? LastAttemptAt { get; set; }
        public TaskCompletionSource<object> CompletionSource { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }

    public enum PrioritySyncType
    {
        AnimeCharacters,
        VoiceActorDetails,
        CharacterDetails
    }

    public enum PrioritySyncPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Urgent = 4 // User is actively waiting
    }

    public class PrioritySyncStatus
    {
        public bool IsRunning { get; set; }
        public int QueueSize { get; set; }
        public int ProcessedToday { get; set; }
        public DateTime? LastProcessedAt { get; set; }
        public TimeSpan? AverageProcessingTime { get; set; }
        public int FailedRequestsToday { get; set; }
        public RateLimitStatus RateLimitStatus { get; set; }
    }

    public class RateLimitStatus
    {
        public int RequestsInCurrentWindow { get; set; }
        public int MaxRequestsPerWindow { get; set; }
        public TimeSpan WindowDuration { get; set; }
        public DateTime WindowStartTime { get; set; }
        public TimeSpan? BackoffDelay { get; set; }
        public bool IsRateLimited { get; set; }
    }

    public class SyncQueueState
    {
        public List<string> PendingAnimeIds { get; set; } = new();
        public List<string> ProcessedAnimeIds { get; set; } = new();
        public List<string> FailedAnimeIds { get; set; } = new();
        public DateTime? LastSyncResumeTime { get; set; }
        public string CurrentSyncId { get; set; }
        public DateTime? SyncStartTime { get; set; }
    }

    public static class SyncMetadataKeys
    {
        public const string LastFullSyncDate = "LastFullSyncDate";
        public const string LastFetchedDate = "LastFetchedDate";
        public const string LastFetchedId = "LastFetchedId";
        public const string SyncInProgress = "SyncInProgress";
        public const string SyncProgressStage = "SyncProgressStage";
        public const string SyncProgressPercent = "SyncProgressPercent";
        public const string PrioritySyncQueueState = "PrioritySyncQueueState";
        public const string RateLimitState = "RateLimitState";
    }
}