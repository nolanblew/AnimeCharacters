using AnimeCharacters.Models;
using AnimeCharacters.Services;
using EventAggregator.Blazor;
using Kitsu.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnimeCharacters.Pages
{
    public partial class ProfileSettings
    {
        [Inject]
        public IProviderManagementService ProviderManagementService { get; set; }

        public User CurrentUser { get; set; }
        public UserSettings Settings { get; set; } = new UserSettings();
        public TitleType SelectedTitleType { get; set; } = TitleType.UserPreferred;
        public string AppVersion { get; set; } = "Loading...";
        public bool UpdateAvailable { get; set; }
        
        // Provider management properties
        public List<ManageableProvider> InstalledProviders { get; set; } = new();
        public Dictionary<string, bool> ProviderCanBeRemoved { get; set; } = new();
        public bool ShowProviderStore { get; set; }

        protected override async Task OnInitializedAsync()
        {
            CurrentUser = await DatabaseProvider.GetUserAsync();
            Settings = await DatabaseProvider.GetUserSettingsAsync() ?? new UserSettings();
            SelectedTitleType = Settings.PreferredTitleType;

            await GetAppVersionAsync();
            await CheckForUpdatesAsync();
            await LoadProviders();

            await base.OnInitializedAsync();
        }

        public async Task HandleAsync(Events.DatabaseEvent arg)
        {
            CurrentUser = await DatabaseProvider.GetUserAsync();
            StateHasChanged();
        }

        async Task OnTitleTypeChanged()
        {
            Settings.PreferredTitleType = SelectedTitleType;
            await DatabaseProvider.SetUserSettingsAsync(Settings);
            
            // Trigger a database event to refresh all components using title preferences
            await _EventAggregator.PublishAsync(new Events.DatabaseEvent());
            await _EventAggregator.PublishAsync(new Events.SnackbarEvent("Settings saved"));
        }

        async Task GetAppVersionAsync()
        {
            try
            {
                var registration = await JSRuntime.InvokeAsync<IJSObjectReference>("navigator.serviceWorker.getRegistration");
                if (registration != null)
                {
                    var activeWorker = await registration.InvokeAsync<IJSObjectReference>("active");
                    if (activeWorker != null)
                    {
                        AppVersion = "2.0.0"; // Default version, could be enhanced to get from service worker
                    }
                }
            }
            catch
            {
                AppVersion = "2.0.0";
            }
        }

        async Task CheckForUpdatesAsync()
        {
            try
            {
                UpdateAvailable = await JSRuntime.InvokeAsync<bool>("checkForUpdates");
            }
            catch
            {
                UpdateAvailable = false;
            }
        }

        async Task UpdateApp()
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("window.location.reload");
            }
            catch
            {
                // Fallback if JS fails
                NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
            }
        }

        async Task LogoutAsync()
        {
            await DatabaseProvider.ClearAsync();
            PageStateManager.Clear();
            NavigationManager.NavigateTo("/?logout=true");
        }

        // Provider management methods
        async Task LoadProviders()
        {
            try
            {
                InstalledProviders = await ProviderManagementService.GetInstalledProvidersAsync();
                
                // Check which providers can be removed
                ProviderCanBeRemoved.Clear();
                foreach (var provider in InstalledProviders)
                {
                    ProviderCanBeRemoved[provider.Id] = await ProviderManagementService.CanRemoveProviderAsync(provider.Id);
                }
                
                StateHasChanged();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error loading providers: {ex.Message}");
                InstalledProviders = new List<ManageableProvider>();
                ProviderCanBeRemoved.Clear();
            }
        }

        void OpenProviderStore()
        {
            ShowProviderStore = true;
            StateHasChanged();
        }

        void CloseProviderStore()
        {
            ShowProviderStore = false;
            StateHasChanged();
        }

        async Task RefreshProviders()
        {
            await LoadProviders();
        }

        async Task HandleRemoveProvider(ManageableProvider provider)
        {
            var success = await ProviderManagementService.RemoveProviderAsync(provider.Id);
            if (success)
            {
                await LoadProviders();
                await _EventAggregator.PublishAsync(new Events.SnackbarEvent($"Removed {provider.Name} provider"));
            }
            else
            {
                await _EventAggregator.PublishAsync(new Events.SnackbarEvent($"Cannot remove {provider.Name} - it's the last active provider"));
            }
        }
    }
}