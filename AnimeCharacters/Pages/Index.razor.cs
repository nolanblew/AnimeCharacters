using AnimeCharacters.Helpers;
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

        string _kitsuUsername = "";

        public string KitsuUsername
        {
            get => _kitsuUsername;
            set
            {
                _kitsuUsername = value;
                _UsernameChanged();
            }
        }

        public string KitsuPassword { get; set; }

        public bool IsPasswordVisible { get; set; }

        public string ErrorLabel { get; set; }

        public User User { get; set; }

        public List<LibraryEntry> LibraryEntries { get; set; }

        public bool IsNotBusy { get; set; } = true;

        public string ButtonLabel => KitsuUsername.Contains("@") ? "Login" : "Continue";

        protected override async Task OnInitializedAsync()
        {
            User = await DatabaseProvider.GetUserAsync();

            var forceLogout = NavigationManager.QueryString("logout") == "true";

            if (forceLogout)
            {
                await _ClearUser();
            }

            if (User != null)
            {
                _GoToAnimes();
            }

            await base.OnInitializedAsync();
        }

        protected async Task _ClearUser()
        {
            User = null;
            KitsuUsername = string.Empty;
            await DatabaseProvider.ClearAsync();
        }

        protected async Task _ButtonClicked()
        {
            if (User == null)
            {
                if (KitsuUsername.Contains("@"))
                {
                    if (!string.IsNullOrWhiteSpace(KitsuPassword))
                    {
                        await _GetKitsuUsernameFromEmail();
                    }
                    else
                    {
                        ErrorLabel = "Please enter a password.";
                    }
                }
                else
                {
                    await _GetKitsuUsername();
                }
            }
            else
            {
                _GoToAnimes();
            }
        }

        protected void _UsernameChanged()
        {
            if (!IsPasswordVisible && KitsuUsername.Contains("@"))
            {
                IsPasswordVisible = true;
            }
        }

        async Task _GetKitsuUsername()
        {
            if (string.IsNullOrWhiteSpace(KitsuUsername))
            {
                ErrorLabel = "Please enter a valid Kitsu username or email address.";
                return;
            }

            IsNotBusy = false;

            try
            {
                var kitsuUser = await _kitsuClient.Users.GetUserAsync(KitsuUsername);

                if (kitsuUser == null)
                {
                    ErrorLabel = "Can't find that user. Try again or login with your email address.";
                    return;
                }

                User = kitsuUser;
                await DatabaseProvider.SetUserAsync(User);

                _GoToAnimes();
            }
            catch (Exception ex)
            {
                ErrorLabel = $"Uh oh, something went wrong: {ex.Message}";
            }
            finally
            {
                IsNotBusy = true;
            }
        }

        async Task _GetKitsuUsernameFromEmail()
        {
            if (!KitsuUsername.Contains("@") || !KitsuUsername.Contains("@"))
            {
                ErrorLabel = "Please enter a valid Kitsu username or email address.";
                return;
            }

            if (KitsuPassword.Length < 3)
            {
                ErrorLabel = "Please enter a valid password.";
                return;
            }

            IsNotBusy = false;

            try
            {
                string authToken;

                try
                {
                    var loginToken = await _kitsuClient.Auth.Login(KitsuUsername, KitsuPassword);

                    if (loginToken == null) { throw new UnauthorizedAccessException("Username and password do not match."); }

                    authToken = loginToken.access_token;
                }
                catch (InvalidCastException ex)
                {
                    ErrorLabel = ex.Message;
                    return;
                }

                var kitsuUser = await _kitsuClient.Users.GetUserSelfAsync(authToken);

                if (kitsuUser == null)
                {
                    ErrorLabel = "Something went wrong. Try again.";
                    return;
                }

                User = kitsuUser;
                await DatabaseProvider.SetUserAsync(User);

                _GoToAnimes();
            }
            catch (Exception ex)
            {
                ErrorLabel = $"Something went wrong: {ex.Message}";
            }
            finally
            {
                IsNotBusy = true;
            }
        }

        void _GoToAnimes()
        {
            PageStateManager.Clear();
            NavigationManager.NavigateTo("animes");
        }
    }
}
