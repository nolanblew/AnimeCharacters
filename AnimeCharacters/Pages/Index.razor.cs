using Blazored.LocalStorage;
using Kitsu;
using Kitsu.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnimeCharacters.Pages
{
    public partial class Index
    {
        readonly KitsuClient _kitsuClient = new();

        [Inject]
        IDatabaseProvider _DatabaseProvider { get; set; }

        [Inject]
        NavigationManager _Navigation { get; set; }

        public string KitsuUsername { get; set; }

        public string StatusLabel { get; set; }

        public User User { get; set; }

        public List<LibraryEntry> LibraryEntries { get; set; }

        public bool IsNotBusy { get; set; } = true;

        public string ButtonLabel => User == null ? "Get User" : "Fetch Anime";

        protected override async Task OnInitializedAsync()
        {
            User = await _DatabaseProvider.GetUserAsync();

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
            await _DatabaseProvider.ClearAsync();

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
                await _DatabaseProvider.SetUserAsync(User);
                
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

        void _GetUserAnime()
        {
            _Navigation.NavigateTo("animes");
        }

        protected void _SetUserStatus()
        {
            StatusLabel = $"Welcome, {User.Name}. Good to see you!";
        }
    }
}
