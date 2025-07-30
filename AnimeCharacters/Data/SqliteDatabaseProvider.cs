using AnimeCharacters.Data.Extensions;
using AnimeCharacters.Data.Models;
using AnimeCharacters.Events;
using AnimeCharacters.Models;
using EventAggregator.Blazor;
using Kitsu.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeCharacters.Data
{
    public class SqliteDatabaseProvider : IDatabaseProvider
    {
        private readonly IDbContextFactory<AnimeCharactersDbContext> _contextFactory;
        private readonly IEventAggregator _eventAggregator;

        public SqliteDatabaseProvider(
            IDbContextFactory<AnimeCharactersDbContext> contextFactory,
            IEventAggregator eventAggregator)
        {
            _contextFactory = contextFactory;
            _eventAggregator = eventAggregator;
        }

        // User operations
        public async ValueTask<User> GetUserAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var dbUser = await context.Users.FirstOrDefaultAsync();
            return dbUser?.ToUser();
        }

        public async ValueTask SetUserAsync(User value)
        {
            if (value == null) return;

            using var context = await _contextFactory.CreateDbContextAsync();
            
            var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Id == value.Id);
            if (existingUser != null)
            {
                existingUser.Name = value.Name ?? "Unknown User";
                existingUser.Username = value.Username ?? $"user_{value.Id}";
                existingUser.AvatarUrl = value.AvatarUrl;
                existingUser.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                context.Users.Add(value.ToDbUser());
            }

            await context.SaveChangesAsync();
            await _TriggerEventAsync();
        }

        // Last Fetched ID operations
        public async ValueTask<long?> GetLastFetchedIdAsnyc()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var metadata = await context.SyncMetadata
                .FirstOrDefaultAsync(m => m.Key == SyncMetadataKeys.LastFetchedId);
            
            return long.TryParse(metadata?.Value, out var id) ? id : null;
        }

        public async ValueTask SetLastFetchedIdAsync(long? value)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var metadata = await context.SyncMetadata
                .FirstOrDefaultAsync(m => m.Key == SyncMetadataKeys.LastFetchedId);
            
            if (metadata != null)
            {
                metadata.Value = value?.ToString();
                metadata.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                context.SyncMetadata.Add(new DbSyncMetadata
                {
                    Key = SyncMetadataKeys.LastFetchedId,
                    Value = value?.ToString()
                });
            }

            await context.SaveChangesAsync();
            await _TriggerEventAsync();
        }

        // Last Fetched Date operations
        public async ValueTask<DateTimeOffset?> GetLastFetchedDateAsnyc()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var metadata = await context.SyncMetadata
                .FirstOrDefaultAsync(m => m.Key == SyncMetadataKeys.LastFetchedDate);
            
            return DateTimeOffset.TryParse(metadata?.Value, out var date) ? date : null;
        }

        public async ValueTask SetLastFetchedDateAsync(DateTimeOffset value)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var metadata = await context.SyncMetadata
                .FirstOrDefaultAsync(m => m.Key == SyncMetadataKeys.LastFetchedDate);
            
            if (metadata != null)
            {
                metadata.Value = value.ToString("O");
                metadata.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                context.SyncMetadata.Add(new DbSyncMetadata
                {
                    Key = SyncMetadataKeys.LastFetchedDate,
                    Value = value.ToString("O")
                });
            }

            await context.SaveChangesAsync();
            await _TriggerEventAsync();
        }

        // Libraries operations
        public async ValueTask<IList<LibraryEntry>> GetLibrariesAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var dbLibraryEntries = await context.LibraryEntries
                .Include(le => le.Anime)
                .Include(le => le.User)
                .ToListAsync();
            
            return dbLibraryEntries.Select(le => le.ToLibraryEntry()).ToList();
        }

        public async ValueTask SetLibrariesAsync(IList<LibraryEntry> value)
        {
            if (value == null) return;

            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Get current user
            var currentUser = await context.Users.FirstOrDefaultAsync();
            if (currentUser == null) return;

            // Remove existing library entries for this user
            var existingEntries = await context.LibraryEntries
                .Where(le => le.UserId == currentUser.Id)
                .ToListAsync();
            context.LibraryEntries.RemoveRange(existingEntries);

            // Add new library entries
            foreach (var libraryEntry in value)
            {
                var dbLibraryEntry = libraryEntry.ToDbLibraryEntry();
                dbLibraryEntry.UserId = currentUser.Id;
                
                // Ensure anime exists
                if (libraryEntry.Anime != null)
                {
                    var existingAnime = await context.Anime
                        .FirstOrDefaultAsync(a => a.KitsuId == libraryEntry.Anime.KitsuId);
                    
                    if (existingAnime == null)
                    {
                        context.Anime.Add(libraryEntry.Anime.ToDbAnime());
                    }
                    else
                    {
                        // Update existing anime
                        var updatedAnime = libraryEntry.Anime.ToDbAnime();
                        existingAnime.Title = updatedAnime.Title;
                        existingAnime.RomanjiTitle = updatedAnime.RomanjiTitle;
                        existingAnime.EnglishTitle = updatedAnime.EnglishTitle;
                        existingAnime.PosterImageUrl = updatedAnime.PosterImageUrl;
                        existingAnime.KitsuSlug = updatedAnime.KitsuSlug;
                        existingAnime.YoutubeId = updatedAnime.YoutubeId;
                        existingAnime.ShowType = updatedAnime.ShowType;
                        existingAnime.UpdatedAt = DateTime.UtcNow;
                    }
                }

                context.LibraryEntries.Add(dbLibraryEntry);
            }

            await context.SaveChangesAsync();
            await _TriggerEventAsync();
        }

        // Migration Version operations
        public async ValueTask<int?> GetMigrationVersionAsnyc()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var metadata = await context.SyncMetadata
                .FirstOrDefaultAsync(m => m.Key == SyncMetadataKeys.MigrationVersion);
            
            return int.TryParse(metadata?.Value, out var version) ? version : null;
        }

        public async ValueTask SetMigrationVersionAsync(int value)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var metadata = await context.SyncMetadata
                .FirstOrDefaultAsync(m => m.Key == SyncMetadataKeys.MigrationVersion);
            
            if (metadata != null)
            {
                metadata.Value = value.ToString();
                metadata.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                context.SyncMetadata.Add(new DbSyncMetadata
                {
                    Key = SyncMetadataKeys.MigrationVersion,
                    Value = value.ToString()
                });
            }

            await context.SaveChangesAsync();
            await _TriggerEventAsync();
        }

        // User Settings operations
        public async ValueTask<UserSettings> GetUserSettingsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var currentUser = await context.Users.FirstOrDefaultAsync();
            if (currentUser == null) return new UserSettings();

            var dbSettings = await context.UserSettings
                .FirstOrDefaultAsync(us => us.UserId == currentUser.Id);
            
            return dbSettings?.ToUserSettings() ?? new UserSettings();
        }

        public async ValueTask SetUserSettingsAsync(UserSettings value)
        {
            if (value == null) return;

            using var context = await _contextFactory.CreateDbContextAsync();
            
            var currentUser = await context.Users.FirstOrDefaultAsync();
            if (currentUser == null) return;

            var existingSettings = await context.UserSettings
                .FirstOrDefaultAsync(us => us.UserId == currentUser.Id);
            
            if (existingSettings != null)
            {
                existingSettings.PreferredTitleType = value.PreferredTitleType;
                existingSettings.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                context.UserSettings.Add(value.ToDbUserSettings(currentUser.Id));
            }

            await context.SaveChangesAsync();
            await _TriggerEventAsync();
        }

        // Clear all data
        public async ValueTask ClearAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Clear all tables in the correct order to avoid foreign key constraints
            context.UserSettings.RemoveRange(context.UserSettings);
            context.SyncMetadata.RemoveRange(context.SyncMetadata);
            context.AnimeCharacters.RemoveRange(context.AnimeCharacters);
            context.CharacterVoiceActors.RemoveRange(context.CharacterVoiceActors);
            context.LibraryEntries.RemoveRange(context.LibraryEntries);
            context.VoiceActors.RemoveRange(context.VoiceActors);
            context.Characters.RemoveRange(context.Characters);
            context.Anime.RemoveRange(context.Anime);
            context.Users.RemoveRange(context.Users);

            await context.SaveChangesAsync();
            await _TriggerEventAsync();
        }

        private async Task _TriggerEventAsync()
        {
            await _eventAggregator.PublishAsync(new DatabaseEvent());
        }
    }
}