using AnimeCharacters.Comparers;
using AnimeCharacters.Helpers;
using AnimeCharacters.Models;
using Kitsu.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
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
            var libraryEntry = (await DatabaseProvider.GetLibrariesAsync()).FirstOrDefault(libray => libray.Anime?.KitsuId == Id);
            CurrentAnime = libraryEntry?.Anime;

            if (CurrentAnime == null)
            {
                NavigationManager.NavigateTo("/");
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentAnime.AnilistId))
            {
                NavigationManager.NavigateTo("/animes");
                return;
            }

            StateHasChanged();

            await _LoadCharacters();

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
            // Use the priority sync service to get characters (immediate return if cached, or priority sync if not)
            var characters = await SyncService.RequestAnimeCharactersSyncAsync(
                CurrentAnime.KitsuId, 
                Data.Services.PrioritySyncPriority.Urgent); // User is actively waiting

            var expandedCharacters = new List<AniListClient.Models.Character>();

            foreach (var character in characters)
            {
                if (character.VoiceActors?.Any() == true)
                {
                    foreach (var va in character.VoiceActors)
                    {
                        // clone character but only include the current voice actor
                        expandedCharacters.Add(character with { VoiceActors = new List<AniListClient.Models.VoiceActorSlim> { va } });
                    }
                }
                else
                {
                    expandedCharacters.Add(character);
                }
            }

            CharactersList = expandedCharacters
                .OrderByDescending(c => c, CharacterByRoleComparer.Instance)
                .ToList();
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
