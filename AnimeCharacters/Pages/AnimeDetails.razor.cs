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

        public bool IsLoadingCharacters { get; set; }

        public string CharacterLoadError { get; set; }

        public UserSettings UserSettings { get; set; } = new UserSettings();

        [Parameter]
        public string Id { get; set; }

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
            var libraries = await DatabaseProvider.GetLibrariesAsync() ?? new List<LibraryEntry>();
            var currentLibraryEntry = libraries.FirstOrDefault(library => library.Anime?.KitsuId == Id);
            CurrentAnime = currentLibraryEntry?.Anime;

            if (CurrentAnime == null)
            {
                NavigationManager.NavigateTo("/");
                return;
            }

            if (!await _EnsureAniListId(libraries))
            {
                CharacterLoadError = "AniList has not linked this anime yet, so characters cannot be loaded.";
                StateHasChanged();
                return;
            }

            StateHasChanged();

            IsLoadingCharacters = true;
            CharacterLoadError = null;
            try
            {
                await _LoadCharacters();
            }
            catch (Exception ex)
            {
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

        async Task<bool> _EnsureAniListId(IList<LibraryEntry> libraries)
        {
            if (!string.IsNullOrWhiteSpace(CurrentAnime.AnilistId))
            {
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
                await DatabaseProvider.SetLibrariesAsync(libraries);
                return true;
            }

            return false;
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
