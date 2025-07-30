using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AnimeCharacters.Data.Services
{
    public interface ISyncService
    {
        /// <summary>
        /// Performs a full synchronization of all user data
        /// </summary>
        Task<SyncResult> PerformFullSyncAsync(SyncProgressCallback progressCallback = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Performs a delta synchronization to update only changed data
        /// </summary>
        Task<SyncResult> PerformDeltaSyncAsync(SyncProgressCallback progressCallback = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the current sync status
        /// </summary>
        Task<SyncStatus> GetSyncStatusAsync();
        
        /// <summary>
        /// Cancels the current sync operation if running
        /// </summary>
        Task CancelSyncAsync();
        
        /// <summary>
        /// Resumes a previously interrupted sync operation
        /// </summary>
        Task<SyncResult> ResumeSyncAsync(SyncProgressCallback progressCallback = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Determines the appropriate sync strategy based on current state
        /// </summary>
        Task<SyncStrategy> DetermineSyncStrategyAsync();

        /// <summary>
        /// Requests priority sync for specific anime characters (returns immediately with cached data if available)
        /// </summary>
        Task<List<AniListClient.Models.Character>> RequestAnimeCharactersSyncAsync(string animeKitsuId, PrioritySyncPriority priority = PrioritySyncPriority.Normal, CancellationToken cancellationToken = default);

        /// <summary>
        /// Requests priority sync for voice actor details (returns immediately with cached data if available)
        /// </summary>
        Task<AniListClient.Models.Staff> RequestVoiceActorSyncAsync(int voiceActorId, PrioritySyncPriority priority = PrioritySyncPriority.Normal, CancellationToken cancellationToken = default);

        /// <summary>
        /// Starts background priority sync processing
        /// </summary>
        Task StartBackgroundSyncAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stops background priority sync processing
        /// </summary>
        Task StopBackgroundSyncAsync();
    }

    public delegate Task SyncProgressCallback(SyncProgress progress);

    public class SyncResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public SyncStrategy Strategy { get; set; }
        public TimeSpan Duration { get; set; }
        public SyncStatistics Statistics { get; set; }
    }

    public class SyncProgress
    {
        public SyncStage Stage { get; set; }
        public string StageName { get; set; }
        public int CurrentStep { get; set; }
        public int TotalSteps { get; set; }
        public double ProgressPercentage => TotalSteps > 0 ? (double)CurrentStep / TotalSteps * 100 : 0;
        public string CurrentOperation { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan? EstimatedTimeRemaining { get; set; }
    }

    public class SyncStatus
    {
        public bool IsSyncInProgress { get; set; }
        public SyncStage? CurrentStage { get; set; }
        public double ProgressPercentage { get; set; }
        public DateTime? LastSyncDate { get; set; }
        public DateTime? LastFullSyncDate { get; set; }
        public SyncStrategy? LastSyncStrategy { get; set; }
        public bool CanResume { get; set; }
    }

    public class SyncStatistics
    {
        public int AnimeSynced { get; set; }
        public int CharactersSynced { get; set; }
        public int VoiceActorsSynced { get; set; }
        public int LibraryEntriesUpdated { get; set; }
        public int NewItemsAdded { get; set; }
        public int ItemsUpdated { get; set; }
        public long DataTransferred { get; set; } // bytes
    }

    public enum SyncStrategy
    {
        FullSync,
        DeltaSync,
        Resume
    }

    public enum SyncStage
    {
        Initializing,
        FetchingUserData,
        FetchingLibraries,
        FetchingCharacters,
        FetchingVoiceActors,
        UpdatingDatabase,
        Finalizing,
        Completed,
        Error
    }
}