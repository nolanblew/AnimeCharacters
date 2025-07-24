using System;
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