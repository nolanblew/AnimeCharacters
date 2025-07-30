using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnimeCharacters.Services
{
    /// <summary>
    /// Represents a provider that can be managed in the UI
    /// </summary>
    public class ManageableProvider
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        public string Category { get; set; } // "Anime Library", "Character Data", etc.
        public bool IsInstalled { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsRemovable { get; set; } // Some providers like Kitsu might be core and non-removable
        public int Priority { get; set; }
        public string Version { get; set; }
        public List<string> Features { get; set; } = new();
    }

    /// <summary>
    /// Service for managing providers in the UI
    /// </summary>
    public interface IProviderManagementService
    {
        /// <summary>
        /// Gets all available providers (installed and available to install)
        /// </summary>
        Task<List<ManageableProvider>> GetAllProvidersAsync();

        /// <summary>
        /// Gets only installed providers
        /// </summary>
        Task<List<ManageableProvider>> GetInstalledProvidersAsync();

        /// <summary>
        /// Gets providers available to install
        /// </summary>
        Task<List<ManageableProvider>> GetAvailableProvidersAsync();

        /// <summary>
        /// Installs a provider
        /// </summary>
        Task<bool> InstallProviderAsync(string providerId);

        /// <summary>
        /// Removes a provider
        /// </summary>
        Task<bool> RemoveProviderAsync(string providerId);

        /// <summary>
        /// Enables/disables a provider
        /// </summary>
        Task<bool> SetProviderEnabledAsync(string providerId, bool enabled);

        /// <summary>
        /// Updates provider priority
        /// </summary>
        Task<bool> SetProviderPriorityAsync(string providerId, int priority);

        /// <summary>
        /// Checks if a provider can be removed (not the last active provider)
        /// </summary>
        Task<bool> CanRemoveProviderAsync(string providerId);
    }
}