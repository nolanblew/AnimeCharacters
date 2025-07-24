using System;
using System.ComponentModel.DataAnnotations;

namespace AnimeCharacters.Data.Models
{
    public class DbSyncMetadata
    {
        [Key]
        [MaxLength(100)]
        public string Key { get; set; }
        
        public string Value { get; set; }
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
    
    public static class SyncMetadataKeys
    {
        public const string LastFetchedId = "last_fetched_id";
        public const string LastFetchedDate = "last_fetched_date";
        public const string LastFullSyncDate = "last_full_sync_date";
        public const string SyncInProgress = "sync_in_progress";
        public const string SyncProgressStage = "sync_progress_stage";
        public const string SyncProgressPercent = "sync_progress_percent";
        public const string MigrationVersion = "migration_version";
    }
}