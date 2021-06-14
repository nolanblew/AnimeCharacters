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
    public partial class Animes
    {
        readonly KitsuClient _kitsuClient = new();

        [Inject]
        public UserSettingsProvider UserSettingsProvider { get; set; }

        [Inject]
        public NavigationManager Navigation { get; set; }

        public User CurrentUser { get; set; }
        public List<LibraryEntry> LibraryEntries { get; set; } = new List<LibraryEntry>();

        protected override async Task OnInitializedAsync()
        {
            var settings = await UserSettingsProvider.Get();
            CurrentUser = settings.CurrentUser;

            if (CurrentUser == null)
            {
                // Redirect to login page
                Navigation.NavigateTo("/");
                return;
            }

            await _GetUserAnime();
        }

        async Task _GetUserAnime()
        {
            if (LibraryEntries.Count > 0)
            {
                //StatusLabel = $"You already pinged for this. You have {LibraryEntries.Count} entries, your most recent one being {mostRecentText}";
                return;
            }

            try
            {
                //IsNotBusy = false;
                //StatusLabel = "Loading your anime collection...";
                var stopwatch = Stopwatch.StartNew();

                //LibraryEntries = _GetLibraryEntries();

                LibraryEntries =
                    (await _kitsuClient.UserLibraries.GetCompleteLibraryCollectionAsync
                        (CurrentUser.Id,
                        Kitsu.Controllers.LibraryType.Anime,
                        Kitsu.Controllers.LibraryStatus.Current | Kitsu.Controllers.LibraryStatus.Completed))
                    .OrderByDescending(e => e.ProgressedAt)
                    .ToList();

                stopwatch.Stop();
                //StatusLabel = $"Finished in {stopwatch.Elapsed}! You have {LibraryEntries.Count} entries, your most recent one being {mostRecentText}";
                return;
            }
            catch (Exception ex)
            {
                //StatusLabel = $"Something went wrong: {ex.Message}";
            }
            finally
            {
                //IsNotBusy = true;
            }
        }

        List<LibraryEntry> _GetLibraryEntries() =>
            Enumerable.Repeat(
                new LibraryEntry
                {
                    Id = 0,
                    Progress = 1,
                    Status = Kitsu.Controllers.LibraryStatus.Current,
                    Type = Kitsu.Controllers.LibraryType.Anime,
                    StartedAt = DateTime.Now,
                    ProgressedAt = DateTime.Now,
                    Anime = new Anime
                    {
                        EnglishTitle = "Currently Watching",
                        Title = "Loading...",
                        PosterImageUrl = "https://via.placeholder.com/150x215.png?text=Loading...",
                        ShowType = AnimeType.Show,
                    }
                }, 8)
            .Concat(
                Enumerable.Repeat(
                    new LibraryEntry
                    {
                        Id = 0,
                        Progress = 1,
                        Status = Kitsu.Controllers.LibraryStatus.Completed,
                        Type = Kitsu.Controllers.LibraryType.Anime,
                        StartedAt = DateTime.Now,
                        ProgressedAt = DateTime.Now,
                        Anime = new Anime
                        {
                            EnglishTitle = "The longest anime title that I have right now is currently loading up a storm",
                            Title = "Loading...",
                            PosterImageUrl = "https://via.placeholder.com/150x215.png?text=Loading...",
                            ShowType = AnimeType.Show,
                        }
                    }, 100))
            .ToList();
    }
}
