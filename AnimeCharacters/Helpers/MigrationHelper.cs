namespace AnimeCharacters.Helpers
{
    public static class MigrationHelper
    {
        public const int CURRENT_MIGRATION_VERSION = 1;

        public static bool IsOnLatestVersion(int? lastSavedVersion)
        {
            return lastSavedVersion != null && lastSavedVersion.Value >= CURRENT_MIGRATION_VERSION;
        }
    }
}
