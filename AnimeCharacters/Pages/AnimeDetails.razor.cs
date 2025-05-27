using AnimeCharacters.Comparers;
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
            CurrentAnime = (await DatabaseProvider.GetLibrariesAsync()).FirstOrDefault(libray => libray.Anime?.KitsuId == Id).Anime;

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
            var media = await AnilistClient.Characters.GetMediaWithCharactersById(int.Parse(CurrentAnime.AnilistId));
            CharactersList = media.Characters
                .SelectMany(character =>
                    (character.VoiceActors != null && character.VoiceActors.Any())
                        ? character.VoiceActors.Select(va => character with { VoiceActors = new List<AniListClient.Models.VoiceActorSlim> { va } })
                        : new[] { character }
                )
                .ToList();
        }
    }
}
