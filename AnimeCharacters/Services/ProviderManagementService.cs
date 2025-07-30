using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeCharacters.Services
{
    /// <summary>
    /// Default implementation of provider management service
    /// </summary>
    public class ProviderManagementService : IProviderManagementService
    {
        private readonly IDataProviderService _dataProviderService;
        private readonly IDatabaseProvider _databaseProvider;

        // Only user-manageable providers are listed here
        // AniList is a backend dependency and not shown to users
        private readonly List<ManageableProvider> _availableProviders = new()
        {
            new ManageableProvider
            {
                Id = "kitsu",
                Name = "Kitsu",
                Description = "Access your anime library from Kitsu.app, the most comprehensive anime tracking platform.",
                LogoUrl = "/kitsu_logo.png",
                Category = "Anime Library",
                IsInstalled = true,
                IsEnabled = true,
                IsRemovable = true, // Can be removed if other providers are available
                Priority = 100,
                Version = "2.0.0",
                Features = new List<string> { "Anime Library", "Progress Tracking", "Library Sync" }
            }
            // Future providers will be added here when they're actually implemented
        };

        public ProviderManagementService(IDataProviderService dataProviderService, IDatabaseProvider databaseProvider)
        {
            _dataProviderService = dataProviderService;
            _databaseProvider = databaseProvider;
        }

        public async Task<List<ManageableProvider>> GetAllProvidersAsync()
        {
            // In a real implementation, this might load from database and merge with available providers
            var userSettings = await GetUserProviderSettings();
            
            return _availableProviders.Select(p =>
            {
                var userSetting = userSettings.FirstOrDefault(us => us.ProviderId == p.Id);
                if (userSetting != null)
                {
                    p.IsEnabled = userSetting.IsEnabled;
                    p.Priority = userSetting.Priority;
                }
                return p;
            }).ToList();
        }

        public async Task<List<ManageableProvider>> GetInstalledProvidersAsync()
        {
            var allProviders = await GetAllProvidersAsync();
            return allProviders.Where(p => p.IsInstalled).ToList();
        }

        public async Task<List<ManageableProvider>> GetAvailableProvidersAsync()
        {
            var allProviders = await GetAllProvidersAsync();
            return allProviders.Where(p => !p.IsInstalled).ToList();
        }

        public async Task<bool> InstallProviderAsync(string providerId)
        {
            // For now, just mark as installed in our static list
            // In a real implementation, this would download and register the provider
            var provider = _availableProviders.FirstOrDefault(p => p.Id == providerId);
            if (provider != null)
            {
                provider.IsInstalled = true;
                provider.IsEnabled = true;
                await SaveUserProviderSetting(providerId, true, provider.Priority);
                return true;
            }
            return false;
        }

        public async Task<bool> RemoveProviderAsync(string providerId)
        {
            var provider = _availableProviders.FirstOrDefault(p => p.Id == providerId);
            if (provider != null && provider.IsRemovable)
            {
                // Check if this is the last active provider
                var installedProviders = _availableProviders.Where(p => p.IsInstalled && p.IsEnabled).ToList();
                if (installedProviders.Count <= 1)
                {
                    // Cannot remove the last provider
                    return false;
                }

                provider.IsInstalled = false;
                provider.IsEnabled = false;
                await SaveUserProviderSetting(providerId, false, provider.Priority);
                return true;
            }
            return false;
        }

        public async Task<bool> SetProviderEnabledAsync(string providerId, bool enabled)
        {
            var provider = _availableProviders.FirstOrDefault(p => p.Id == providerId);
            if (provider != null && provider.IsInstalled)
            {
                provider.IsEnabled = enabled;
                await SaveUserProviderSetting(providerId, enabled, provider.Priority);
                return true;
            }
            return false;
        }

        public async Task<bool> SetProviderPriorityAsync(string providerId, int priority)
        {
            var provider = _availableProviders.FirstOrDefault(p => p.Id == providerId);
            if (provider != null && provider.IsInstalled)
            {
                provider.Priority = priority;
                await SaveUserProviderSetting(providerId, provider.IsEnabled, priority);
                return true;
            }
            return false;
        }

        public async Task<bool> CanRemoveProviderAsync(string providerId)
        {
            var provider = _availableProviders.FirstOrDefault(p => p.Id == providerId);
            if (provider == null || !provider.IsRemovable || !provider.IsInstalled)
                return false;

            // Check if this is the last active provider
            var installedProviders = _availableProviders.Where(p => p.IsInstalled && p.IsEnabled).ToList();
            return installedProviders.Count > 1;
        }

        private async Task<List<UserProviderSetting>> GetUserProviderSettings()
        {
            // For now, return empty list. In future, load from database
            // This would be stored in the user's settings
            return new List<UserProviderSetting>();
        }

        private async Task SaveUserProviderSetting(string providerId, bool isEnabled, int priority)
        {
            // For now, do nothing. In future, save to database
            // This would update the user's provider preferences
            await Task.CompletedTask;
        }

        private class UserProviderSetting
        {
            public string ProviderId { get; set; }
            public bool IsEnabled { get; set; }
            public int Priority { get; set; }
        }
    }
}