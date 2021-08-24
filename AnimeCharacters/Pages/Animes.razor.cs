using AnimeCharacters.Helpers;
using Kitsu;
using Kitsu.Comparers;
using Kitsu.Controllers;
using Kitsu.Helpers;
using Kitsu.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeCharacters.Pages
{
    public partial class Animes
    {
        const int _CACHE_UPDATE_TIME_MINUTES =
#if DEBUG
            0;
#else
            1;
#endif

        const int _CACHE_REFRESH_TIME_FORCE_REFRESH_DAYS = 10;

        readonly KitsuClient _kitsuClient = new();

        bool _isMigrating = false;

        Dictionary<long, LibraryEntry> _LibraryEntries { get; set; }

        public User CurrentUser { get; set; }

        public string SearchFilter { get; set; }

        public bool IsBusy { get; set; } = true;

        public List<LibraryEntry> FilteredLibraryEntries
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SearchFilter))
                {
                    return _LibraryEntries?
                    .Values?
                    .OrderByDescending(e => e.ProgressedAt)?
                    .ToList();
                }

                var lowerFilter = SearchFilter.ToLower();
                return _LibraryEntries?.Values?
                    .Where(e => _MatchesSearch(e.Anime))?
                    .OrderByDescending(e => e.ProgressedAt)
                    .ToList();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            CurrentUser = await DatabaseProvider.GetUserAsync();

            if (CurrentUser == null)
            {
                // Redirect to login page
                NavigationManager.NavigateTo("/");
                return;
            }

            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                // Check if we need to migrate
                // TODO: Remove after everyone has updated to latest version
                _isMigrating = !MigrationHelper.IsOnLatestVersion(await DatabaseProvider.GetMigrationVersionAsnyc());

                if (_isMigrating)
                {
                    _LibraryEntries = new();
                }
                else
                {
                    _LibraryEntries = ((await DatabaseProvider.GetLibrariesAsync()) ?? new List<LibraryEntry>())
                        .ToDictionary(lib => lib.Id);
                }

                StateHasChanged();

                await _FetchLibraries();
            }
        }

        protected void _Anime_OnClicked(LibraryEntry libraryEntry)
        {
            if (libraryEntry?.Anime == null) { return; }

            NavigationManager.NavigateTo($"/animes/{libraryEntry.Anime.KitsuId}");
        }

        async Task _FetchLibraries(bool forceFullRefresh = false)
        {
            try
            {
                IsBusy = true;

                if (_isMigrating) { forceFullRefresh = true; }

                var lastFetchedDate = await DatabaseProvider.GetLastFetchedDateAsnyc();

                if (forceFullRefresh
                    || lastFetchedDate == null
                    || DateTimeOffset.Now.Subtract(TimeSpan.FromDays(_CACHE_REFRESH_TIME_FORCE_REFRESH_DAYS)) > lastFetchedDate
                    || !_LibraryEntries.Any())
                {
                    Console.WriteLine($"PATH: Fetching full list from the start.");
                    await _FetchAllUserAnime();

                    if (_isMigrating)
                    {
                        await DatabaseProvider.SetMigrationVersionAsync(MigrationHelper.CURRENT_MIGRATION_VERSION);
                        _isMigrating = false;

                        await _EventAggregator.PublishAsync(new Events.SnackbarEvent("Updated to the latest version."));
                    }

                    return;
                }

                if (DateTimeOffset.Now.Subtract(TimeSpan.FromMinutes(_CACHE_UPDATE_TIME_MINUTES)) > lastFetchedDate)
                {
                    Console.WriteLine($"PATH: Fetching only updates from the start.");
                    await _UpdateUserAnime();
                    return;
                }
            }
            finally
            {
                IsBusy = false;
                StateHasChanged();
            }
        }

        async Task _FetchAllUserAnime()
        {
            try
            {
                _LibraryEntries =
                    (await _kitsuClient.UserLibraries.GetCompleteLibraryCollectionAsync
                        (CurrentUser.Id,
                        LibraryType.Anime,
                        LibraryStatus.Current | LibraryStatus.Completed))
                    .ToDictionary(lib => lib.Id);

                await DatabaseProvider.SetLibrariesAsync(_LibraryEntries.Values.ToList());
                await _SetLastFetchedId();
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in _FetchAllUserAnime(): {ex.Message}");
                Console.WriteLine($" STACKTRACE: {ex.StackTrace}");
                await _EventAggregator.PublishAsync(new Events.SnackbarEvent("Error updating library. Please refresh page."));
            }
        }

        async Task _FetchAnimeIds(long[] ids, bool saveLibrary = true)
        {
            try
            {
                var updatedLibraries =
                    await _kitsuClient.UserLibraries.GetLibraryCollectionByIdsAsync
                        (CurrentUser.Id,
                        ids,
                        LibraryType.Anime,
                        LibraryStatus.Current | LibraryStatus.Completed);

                var libraryDeltas = _LibraryEntries
                    .Where(le => ids.Contains(le.Key))
                    .Select(le => le.Value)
                    .GetDelta(updatedLibraries, l => l.Id, LibraryComparer.Default);

                foreach(var newItem in libraryDeltas.Added)
                {
                    _LibraryEntries.Add(newItem.Id, newItem);
                }

                foreach(var oldItem in libraryDeltas.Deleted)
                {
                    _LibraryEntries.Remove(oldItem.Id);
                }

                foreach(var updatedItem in libraryDeltas.Updated)
                {
                    _LibraryEntries[updatedItem.Id] = updatedItem;
                }

                if (saveLibrary)
                {
                    await DatabaseProvider.SetLibrariesAsync(_LibraryEntries.Values.ToList());
                }

                await _SetLastFetchedId();
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in _FetchAllUserAnime(): {ex.Message}");
                Console.WriteLine($" STACKTRACE: {ex.StackTrace}");
                await _EventAggregator.PublishAsync(new Events.SnackbarEvent("Error updating library. Please refresh page."));
            }
        }

        async Task _UpdateUserAnime()
        {
            void updateLibraryEntry(LibraryEntrySlim libraryEntrySlim)
            {
                var libraryEntry = _LibraryEntries[libraryEntrySlim.Id];
                libraryEntry.Progress = libraryEntrySlim.Progress;
                libraryEntry.ProgressedAt = libraryEntrySlim.ProgressedAt;
                libraryEntry.IsReconsuming = libraryEntrySlim.IsReconsuming;
                libraryEntry.Status = libraryEntrySlim.Status;
                libraryEntry.FinishedAt = libraryEntrySlim.FinishedAt;
            }

            var lastFetchedId = await DatabaseProvider.GetLastFetchedIdAsnyc();

            if (!_LibraryEntries.Any() || !lastFetchedId.HasValue)
            {
                Console.WriteLine($"PATH: No library entries (or no lastFetchedId = {lastFetchedId?.ToString() ?? "[null]"})");
                await _FetchAllUserAnime();
                return;
            }

            try
            {
                var deltaLibraryEvents = await _kitsuClient.UserLibraryEvents.GetLibraryEventsSinceTime(CurrentUser.Id, lastFetchedId.Value);

                // TODO: This is temporary - if there are any added library entries, we need to re-fetch all the libraries and then add the ones that have been added.
                // We should just query the individual libraries we didn't have already

                var idsToAdd = deltaLibraryEvents.LibraryEntryEvents
                    .Where(e => !e.LibraryEntryId.HasValue || !_LibraryEntries.ContainsKey(e.LibraryEntryId.Value))
                    .ToArray();

                bool hasChanges = false;

                if (idsToAdd.Any())
                {
                    Console.WriteLine($"PATH: There were {idsToAdd.Length} items to add.");
                    await _FetchAnimeIds(idsToAdd.Select(l => l.LibraryEntryId.Value).ToArray(), saveLibrary: false);
                    hasChanges = true;
                }

                foreach (var libraryEvent in deltaLibraryEvents.LibraryEntryEvents)
                {
                    if (_LibraryEntries.ContainsKey(libraryEvent.LibraryEntryId.Value))
                    {
                        if (libraryEvent.Type == LibraryEntryEvent.EventType.Updated)
                        {
                            updateLibraryEntry(libraryEvent.LibraryEntrySlim);
                            hasChanges = true;
                        }
                        else if (libraryEvent.Type == LibraryEntryEvent.EventType.Removed)
                        {
                            _LibraryEntries.Remove(libraryEvent.LibraryEntryId.Value);
                            hasChanges = true;
                        }
                        else if (libraryEvent.Type == LibraryEntryEvent.EventType.Completed)
                        {
                            updateLibraryEntry(libraryEvent.LibraryEntrySlim);
                            hasChanges = true;
                        }
                    }
                }

                await DatabaseProvider.SetLastFetchedIdAsync(deltaLibraryEvents.LastFetchedId);
                await DatabaseProvider.SetLastFetchedDateAsync(DateTimeOffset.Now);

                if (hasChanges)
                {
                    await DatabaseProvider.SetLibrariesAsync(_LibraryEntries.Values.ToList());
                    await _EventAggregator.PublishAsync(new Events.SnackbarEvent("Updated your library."));
                }
            }
            catch (RequestCapacityExceededException)
            {
                // There were too many events to delta, so let's just refresh the entire library
                Console.WriteLine("PATH: Too many entries in diff. Fetching all animes in library.");
                await _FetchAllUserAnime();
                return;
            }
            catch (Exception ex)
            {
                await _EventAggregator.PublishAsync(new Events.SnackbarEvent("Error updating library. Please refresh page."));
                Console.WriteLine($"ERROR: {ex.GetType().Name} - {ex.Message}");
                Console.WriteLine($"Stacktrace: {ex.StackTrace}");
            }
        }

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

        async Task _SetLastFetchedId()
        {
            // Ensure we have some library entries, otherwise
            // there's no point in setting up the Sync endpoint
            if (CurrentUser != null && _LibraryEntries.Any())
            {
                var lastEventId = await _kitsuClient.UserLibraryEvents.GetLatestLibraryEventId(CurrentUser.Id);
                await DatabaseProvider.SetLastFetchedIdAsync(lastEventId);
                await DatabaseProvider.SetLastFetchedDateAsync(DateTimeOffset.Now);
            }
        }
    }
}
