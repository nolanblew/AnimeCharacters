using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeCharacters.Data
{
    public interface IDatabaseInitializationService
    {
        Task InitializeDatabaseAsync();
        Task MigrateFromLocalStorageAsync();
    }

    public class DatabaseInitializationService : IDatabaseInitializationService
    {
        private readonly IDbContextFactory<AnimeCharactersDbContext> _contextFactory;
        private readonly IDatabaseProvider _localStorageProvider;
        private readonly SqliteDatabaseProvider _sqliteProvider;

        public DatabaseInitializationService(
            IDbContextFactory<AnimeCharactersDbContext> contextFactory,
            IDatabaseProvider localStorageProvider,
            SqliteDatabaseProvider sqliteProvider)
        {
            _contextFactory = contextFactory;
            _localStorageProvider = localStorageProvider;
            _sqliteProvider = sqliteProvider;
        }

        public async Task InitializeDatabaseAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();
            
            // Apply any pending migrations
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                await context.Database.MigrateAsync();
            }
        }

        public async Task MigrateFromLocalStorageAsync()
        {
            // Check if migration has already been completed
            var migrationVersion = await _sqliteProvider.GetMigrationVersionAsnyc();
            if (migrationVersion >= 1) return; // Already migrated

            // Check if there's data in LocalStorage to migrate
            var user = await _localStorageProvider.GetUserAsync();
            if (user == null) 
            {
                // No data to migrate, just mark as migrated
                await _sqliteProvider.SetMigrationVersionAsync(1);
                return;
            }

            try
            {
                // Migrate user data
                await _sqliteProvider.SetUserAsync(user);

                // Migrate sync metadata
                var lastFetchedId = await _localStorageProvider.GetLastFetchedIdAsnyc();
                if (lastFetchedId.HasValue)
                {
                    await _sqliteProvider.SetLastFetchedIdAsync(lastFetchedId.Value);
                }

                var lastFetchedDate = await _localStorageProvider.GetLastFetchedDateAsnyc();
                if (lastFetchedDate.HasValue)
                {
                    await _sqliteProvider.SetLastFetchedDateAsync(lastFetchedDate.Value);
                }

                // Migrate library data
                var libraries = await _localStorageProvider.GetLibrariesAsync();
                if (libraries != null && libraries.Any())
                {
                    await _sqliteProvider.SetLibrariesAsync(libraries);
                }

                // Migrate user settings
                var userSettings = await _localStorageProvider.GetUserSettingsAsync();
                if (userSettings != null)
                {
                    await _sqliteProvider.SetUserSettingsAsync(userSettings);
                }

                // Mark migration as completed
                await _sqliteProvider.SetMigrationVersionAsync(1);

                // Optionally clear LocalStorage after successful migration
                // await _localStorageProvider.ClearAsync();
            }
            catch (Exception ex)
            {
                // Log error but don't fail the application
                Console.WriteLine($"Migration from LocalStorage failed: {ex.Message}");
                // Could implement retry logic or fallback to LocalStorage
            }
        }
    }
}