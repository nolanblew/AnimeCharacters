using AnimeCharacters.Comparers;
using AnimeCharacters.Helpers;
using AnimeCharacters.Models;
using Kitsu.Models;
using Microsoft.AspNetCore.Components;
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
        AniListClient.AniListClient AnilistClient { get; set; }

        public User CurrentUser { get; set; }

        public Anime CurrentAnime { get; set; }

        public string Language { get; set; } = "Japanese";

        public List<AniListClient.Models.Character> CharactersList { get; set; } = new();

        public bool IsLoadingCharacters { get; set; } = true;

        public string CharacterLoadError { get; set; }

        public UserSettings UserSettings { get; set; } = new UserSettings();

        bool _usingResolvedAniListId;

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

            CurrentUser = await DatabaseProvider.GetUserAsync();
            UserSettings = await DatabaseProvider.GetUserSettingsAsync() ?? new UserSettings();

            if (CurrentAnime == null)
            {
                var libraries = await DatabaseProvider.GetLibrariesAsync() ?? new List<LibraryEntry>();
                var currentLibraryEntry = libraries.FirstOrDefault(library => library.Anime?.KitsuId == Id);
                CurrentAnime = currentLibraryEntry?.Anime;
            }

            if (CurrentAnime == null)
            {
                NavigationManager.NavigateTo("/");
                return;
            }

            IsLoadingCharacters = true;
            CharacterLoadError = null;
            StateHasChanged();

            if (!await _EnsureAniListId())
            {
                CharacterLoadError = "AniList has not linked this anime yet, so characters cannot be loaded.";
                IsLoadingCharacters = false;
                StateHasChanged();
                return;
            }

            try
            {
                await _LoadCharacters();
            }
            catch (Exception ex)
            {
                await _ForgetResolvedAniListIdAsync();
                CharacterLoadError = $"Unable to load characters from AniList. {ex.Message}";
            }
            finally
            {
                IsLoadingCharacters = false;
            }

            StateHasChanged();
        }

        protected void _Character_OnClicked(AniListClient.Models.Character character)
        {
            var voiceActor = character?.VoiceActors?.FirstOrDefault();

            if (voiceActor == null) { return; }

            NavigationManager.NavigateTo($"/characters/{voiceActor.Id}");
        }

        async Task _LoadCharacters()
        {
            var media = await AnilistClient.Characters.GetMediaWithCharactersById(int.Parse(CurrentAnime.AnilistId));

            if (media == null)
            {
                await _ForgetResolvedAniListIdAsync();
                CharacterLoadError = "AniList did not return character data for this anime.";
                return;
            }

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

        async Task<bool> _EnsureAniListId()
        {
            var resolvedAniListId = await DatabaseProvider.GetResolvedAniListIdAsync(CurrentAnime.KitsuId);

            if (!string.IsNullOrWhiteSpace(CurrentAnime.AnilistId))
            {
                _usingResolvedAniListId = CurrentAnime.AnilistId == resolvedAniListId;
                return true;
            }

            if (!string.IsNullOrWhiteSpace(resolvedAniListId))
            {
                CurrentAnime.AnilistId = resolvedAniListId;
                _usingResolvedAniListId = true;
                return true;
            }

            var searchTitles = new[]
            {
                CurrentAnime.EnglishTitle,
                CurrentAnime.RomanjiTitle,
                CurrentAnime.Title,
                _GetSearchTitleFromSlug(CurrentAnime.KitsuSlug)
            }
            .Where(title => !string.IsNullOrWhiteSpace(title))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

            foreach (var title in searchTitles)
            {
                var media = await AnilistClient.Characters.SearchMediaByTitle(title);

                if (media == null || !_IsTitleMatch(media.Title, searchTitles))
                {
                    continue;
                }

                CurrentAnime.AnilistId = media.Id.ToString();
                _usingResolvedAniListId = true;
                await DatabaseProvider.SetResolvedAniListIdAsync(CurrentAnime.KitsuId, CurrentAnime.AnilistId);
                return true;
            }

            return false;
        }

        async Task _ForgetResolvedAniListIdAsync()
        {
            if (!_usingResolvedAniListId || CurrentAnime == null)
            {
                return;
            }

            await DatabaseProvider.RemoveResolvedAniListIdAsync(CurrentAnime.KitsuId);
            CurrentAnime.AnilistId = null;
            _usingResolvedAniListId = false;
        }

        static string _GetSearchTitleFromSlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return null;
            }

            return Regex.Replace(slug.Replace('-', ' '), @"\s+", " ").Trim();
        }

        static bool _IsTitleMatch(AniListClient.Models.Titles mediaTitle, IEnumerable<string> searchTitles)
        {
            if (mediaTitle == null)
            {
                return false;
            }

            var normalizedSearchTitles = searchTitles
                .Select(_NormalizeTitle)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var mediaTitles = new[]
            {
                mediaTitle.English,
                mediaTitle.Romaji,
                mediaTitle.UserPreferred,
                mediaTitle.Native
            };

            return mediaTitles
                .Select(_NormalizeTitle)
                .Any(title => !string.IsNullOrWhiteSpace(title) && normalizedSearchTitles.Contains(title));
        }

        static string _NormalizeTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            return Regex.Replace(title, @"[^\p{L}\p{N}]+", " ").Trim();
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
