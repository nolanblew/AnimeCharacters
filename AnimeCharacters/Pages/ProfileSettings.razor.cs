using AnimeCharacters.Extensions;
using AnimeCharacters.Models;
using EventAggregator.Blazor;
using Kitsu.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnimeCharacters.Pages
{
    public partial class ProfileSettings
    {
        public User CurrentUser { get; set; }
        public UserSettings Settings { get; set; } = new UserSettings();
        public TitleType SelectedTitleType { get; set; } = TitleType.UserPreferred;
        public IReadOnlyList<MediaExtensionDefinition> Extensions { get; set; } = ExtensionCatalog.All;
        public string AppVersion { get; set; } = "Loading...";
        public bool UpdateAvailable { get; set; }

        protected override async Task OnInitializedAsync()
        {
            CurrentUser = await DatabaseProvider.GetUserAsync();
            Settings = await DatabaseProvider.GetUserSettingsAsync() ?? new UserSettings();
            SelectedTitleType = Settings.PreferredTitleType;
            Extensions = ExtensionCatalog.All;

            await GetAppVersionAsync();
            await CheckForUpdatesAsync();

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

        bool IsExtensionEnabled(MediaExtensionDefinition extension) =>
            Settings?.IsExtensionEnabled(extension) ?? extension.EnabledByDefault;

        async Task OnExtensionChanged(MediaExtensionDefinition extension, ChangeEventArgs args)
        {
            if (extension.IsCore)
            {
                return;
            }

            Settings.SetExtensionEnabled(extension.Id, args.Value is bool value && value);
            await DatabaseProvider.SetUserSettingsAsync(Settings);
            await _EventAggregator.PublishAsync(new Events.DatabaseEvent());
            await _EventAggregator.PublishAsync(new Events.SnackbarEvent("Settings saved"));
        }

        Task GetAppVersionAsync()
        {
            AppVersion = VersionInfo.Version;
            return Task.CompletedTask;
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
    }
}
