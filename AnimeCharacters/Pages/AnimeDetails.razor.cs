using AnimeCharacters.Comparers;
using AnimeCharacters.Helpers;
using AnimeCharacters.Models;
using Kitsu.Models;
using Microsoft.AspNetCore.Components;
using ReferenceApis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AnimeCharacters.Pages
{
    public partial class AnimeDetails
    {
        [Inject]
        IReferenceAnimeService ReferenceAnimeService { get; set; }

        public User CurrentUser { get; set; }

        public Anime CurrentAnime { get; set; }

        public string Language { get; set; } = "Japanese";

        public List<AniListClient.Models.Character> CharactersList { get; set; } = new();

        public bool IsLoadingCharacters { get; set; } = true;

        public string CharacterLoadError { get; set; }

        public UserSettings UserSettings { get; set; } = new UserSettings();

        [Parameter]
        public string Id { get; set; }

        protected override void OnParametersSet()
        {
            if (CurrentAnime == null && !string.IsNullOrWhiteSpace(Id))
            {
                CurrentAnime = PageStateManager.GetSelectedLibraryEntry(Id)?.Anime;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (!firstRender) { return; }

            if (string.IsNullOrWhiteSpace(Id))
            {
                NavigationManager.NavigateTo("/");
                return;
            }

            if (_TryRestorePageState())
            {
                StateHasChanged();
                return;
            }

            CurrentUser = await DatabaseProvider.GetUserAsync();
            UserSettings = await DatabaseProvider.GetUserSettingsAsync() ?? new UserSettings();
            var libraries = await DatabaseProvider.GetLibrariesAsync() ?? new List<LibraryEntry>();
            var currentLibraryEntry = libraries.FirstOrDefault(library => library.Anime?.KitsuId == Id);

            CurrentAnime = currentLibraryEntry?.Anime ?? CurrentAnime;

            if (CurrentAnime == null)
            {
                NavigationManager.NavigateTo("/");
                return;
            }

            IsLoadingCharacters = true;
            CharacterLoadError = null;
            StateHasChanged();

            try
            {
                await _LoadCharacters(libraries);
            }
            catch (Exception ex)
            {
                CharacterLoadError = $"Unable to load characters from the reference APIs. {ex.Message}";
            }
            finally
            {
                IsLoadingCharacters = false;
            }

            _CachePageState();
            StateHasChanged();
        }

        protected void _Character_OnClicked(AniListClient.Models.Character character)
        {
            var voiceActor = character?.VoiceActors?.FirstOrDefault();

            if (voiceActor == null) { return; }

            var name = Uri.EscapeDataString(voiceActor.Name?.Full ?? string.Empty);
            NavigationManager.NavigateTo($"/characters/{voiceActor.ProviderName}/{voiceActor.Id}?name={name}");
        }

        async Task _LoadCharacters(IList<LibraryEntry> libraries)
        {
            var result = await ReferenceAnimeService.GetMediaWithCharactersAsync(CurrentAnime, _GetSearchTitles());
            var media = result.Media;

            if (media == null)
            {
                CharacterLoadError = "The reference APIs did not return character data for this anime.";
                return;
            }

            await _PersistResolvedReferenceId(result, libraries);

            var characters = new List<AniListClient.Models.Character>();

            foreach (var character in media.Characters)
            {
                if (character.VoiceActors?.Any() == true)
                {
                    foreach (var va in character.VoiceActors)
                    {
                        // clone character but only include the current voice actor
                        characters.Add(character with { VoiceActors = new List<AniListClient.Models.VoiceActorSlim> { va } });
                    }
                }
                else
                {
                    characters.Add(character);
                }
            }

            CharactersList = characters
                .OrderByDescending(c => c, CharacterByRoleComparer.Instance)
                .ToList();
        }

        async Task _PersistResolvedReferenceId(ReferenceMediaResult result, IList<LibraryEntry> libraries)
        {
            if (result?.AnimeKey == null)
            {
                return;
            }

            var hasChange = false;

            if (ReferenceAnimeKey.MatchesProvider(result.AnimeKey.ProviderName, ReferenceProviderNames.AniList)
                && string.IsNullOrWhiteSpace(CurrentAnime.AnilistId))
            {
                CurrentAnime.AnilistId = result.AnimeKey.Id;
                hasChange = true;
            }

            if (ReferenceAnimeKey.MatchesProvider(result.AnimeKey.ProviderName, ReferenceProviderNames.Jikan)
                && string.IsNullOrWhiteSpace(CurrentAnime.MyAnimeListId))
            {
                CurrentAnime.MyAnimeListId = result.AnimeKey.Id;
                hasChange = true;
            }

            if (hasChange)
            {
                await DatabaseProvider.SetLibrariesAsync(libraries);
            }
        }

        List<string> _GetSearchTitles()
        {
            return new[]
            {
                CurrentAnime?.EnglishTitle,
                CurrentAnime?.RomanjiTitle,
                CurrentAnime?.Title,
                _GetSearchTitleFromSlug(CurrentAnime?.KitsuSlug)
            }
            .Where(title => !string.IsNullOrWhiteSpace(title))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        }

        bool _TryRestorePageState()
        {
            if (!PageStateManager.TryGetPageState<AnimeDetailsPageState>(NavigationManager.Uri, out var state)
                || state.Id != Id)
            {
                return false;
            }

            CurrentUser = state.CurrentUser;
            CurrentAnime = state.CurrentAnime;
            Language = state.Language;
            CharactersList = state.CharactersList;
            CharacterLoadError = state.CharacterLoadError;
            UserSettings = state.UserSettings;
            IsLoadingCharacters = false;
            return true;
        }

        void _CachePageState()
        {
            PageStateManager.SetPageState(NavigationManager.Uri, new AnimeDetailsPageState
            {
                Id = Id,
                CurrentUser = CurrentUser,
                CurrentAnime = CurrentAnime,
                Language = Language,
                CharactersList = CharactersList,
                CharacterLoadError = CharacterLoadError,
                UserSettings = UserSettings
            });
        }

        class AnimeDetailsPageState
        {
            public string Id { get; set; }
            public User CurrentUser { get; set; }
            public Anime CurrentAnime { get; set; }
            public string Language { get; set; }
            public List<AniListClient.Models.Character> CharactersList { get; set; } = new();
            public string CharacterLoadError { get; set; }
            public UserSettings UserSettings { get; set; } = new();
        }

        static string _GetSearchTitleFromSlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return null;
            }

            return Regex.Replace(slug.Replace('-', ' '), @"\s+", " ").Trim();
        }

        public string GetAnimeTitle()
        {
            return TitleHelper.GetPreferredTitle(CurrentAnime, UserSettings.PreferredTitleType);
        }

        public async Task HandleAsync(Events.DatabaseEvent arg)
        {
            UserSettings = await DatabaseProvider.GetUserSettingsAsync() ?? new UserSettings();
            StateHasChanged();
        }
    }
}
