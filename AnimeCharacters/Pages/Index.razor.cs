using Kitsu;
using Kitsu.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeCharacters.Pages
{
    public partial class Index
    {
        readonly KitsuClient _kitsuClient = new();

        [Inject]
        UserSettingsProvider _UserSettingsProvider { get; set; }

        [Inject]
        public NavigationManager Navigation { get; set; }

        public string KitsuUsername { get; set; }

        public string StatusLabel { get; set; }

        public User User { get; set; }

        public List<LibraryEntry> LibraryEntries { get; set; }

        public bool IsNotBusy { get; set; } = true;

        public string ButtonLabel => User == null ? "Get User" : "Fetch Anime";

        protected override async Task OnInitializedAsync()
        {
            var settings = await _UserSettingsProvider.Get();
            User = settings.CurrentUser;

            if (User != null)
            {
                KitsuUsername = User.Username;
                _SetUserStatus();
            }

            await base.OnInitializedAsync();
        }

        protected async Task _ClearUser()
        {
            User = null;
            KitsuUsername = string.Empty;
            var settings = await _UserSettingsProvider.Get();
            settings.CurrentUser = null;
            await _UserSettingsProvider.Save();

            StatusLabel = "User Cleared.";
        }

        protected async Task _ButtonClicked()
        {
            if (User == null)
            {
                await _GetKitsuUsername();
            }
            else
            {
                _GetUserAnime();
            }
        }

        async Task _GetKitsuUsername()
        {
            if (string.IsNullOrWhiteSpace(KitsuUsername))
            {
                StatusLabel = "Uh oh, we couldn't process that. Please enter a valid Kitsu username.";
                return;
            }

            StatusLabel = $"Loading...";
            IsNotBusy = false;

            try
            {
                var kitsuUser = await _kitsuClient.Users.GetUserAsync(KitsuUsername);

                if (kitsuUser == null)
                {

                    StatusLabel = "Can't find that user. Try again.";
                    return;
                }

                User = kitsuUser;

                var settings = await _UserSettingsProvider.Get();
                settings.CurrentUser = User;
                await _UserSettingsProvider.Save();

                _SetUserStatus();
            }
            catch (Exception ex)
            {
                StatusLabel = $"Uh oh, something went wrong: {ex.Message}";
            }
            finally
            {
                IsNotBusy = true;
            }
        }

        async void _GetUserAnime()
        {
            Navigation.NavigateTo("animes");
        }

        protected void _SetUserStatus()
        {
            StatusLabel = $"Welcome, {User.Name}. Good to see you!";
        }
    }
}
