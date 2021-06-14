using Kitsu;
using Kitsu.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace AnimeCharacters.Pages
{
    public partial class Animes
    {
        readonly KitsuClient _kitsuClient = new();

        [Inject]
        IDatabaseProvider _DatabaseProvider { get; set; }

        [Inject]
        public NavigationManager Navigation { get; set; }

        List<LibraryEntry> _LibraryEntries { get; set; }

        public User CurrentUser { get; set; }

        string _searchFilter;

        public string SearchFilter { get; set; }

        public List<LibraryEntry> FilteredLibraryEntries
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SearchFilter))
                {
                    return _LibraryEntries;
                }

                var lowerFilter = SearchFilter.ToLower();
                return _LibraryEntries?
                    .Where(e => _MatchesSearch(e.Anime))?.ToList();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            CurrentUser = await _DatabaseProvider.GetUserAsync();

            if (CurrentUser == null)
            {
                // Redirect to login page
                Navigation.NavigateTo("/");
                return;
            }

            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
               _LibraryEntries = (await _DatabaseProvider.GetLibrariesAsync())?.ToList() ?? new();

                await _GetUserAnime();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        async Task _GetUserAnime()
        {
            if (_LibraryEntries.Count > 0)
            {
                StateHasChanged();
                return;
            }

            try
            {
                var stopwatch = Stopwatch.StartNew();

                _LibraryEntries =
                    (await _kitsuClient.UserLibraries.GetCompleteLibraryCollectionAsync
                        (CurrentUser.Id,
                        Kitsu.Controllers.LibraryType.Anime,
                        Kitsu.Controllers.LibraryStatus.Current | Kitsu.Controllers.LibraryStatus.Completed))
                    .OrderByDescending(e => e.ProgressedAt)
                    .ToList();

                StateHasChanged();

                await _DatabaseProvider.SetLibrariesAsync(_LibraryEntries);
                await _DatabaseProvider.SetLastFetchedAsync(DateTimeOffset.Now);

                stopwatch.Stop();
                return;
            }
            catch (Exception ex)
            {
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

        bool _MatchesSearch(Anime anime)
        {
            if (anime == null) { return false; }

            var lowerFilter = SearchFilter.ToLower();

            var titleSplit = anime.Title.ToLower().Split(' ');
            if (titleSplit.Any(t => t.StartsWith(lowerFilter)))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(anime.RomanjiTitle))
            {
                var romanjiTitleSplit = anime.Title.ToLower().Split(' ');
                if (romanjiTitleSplit.Any(t => t.StartsWith(lowerFilter)))
                {
                    return true;
                }
            }

            if (!string.IsNullOrWhiteSpace(anime.EnglishTitle))
            {
                var englishTitleSplit = anime.EnglishTitle.ToLower().Split(' ');
                if (englishTitleSplit.Any(t => t.StartsWith(lowerFilter)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
