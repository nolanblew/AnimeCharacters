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
        JikanDotNet.IJikan Jikan { get; set; }

        public User CurrentUser { get; set; }

        public Anime CurrentAnime { get; set; }

        public string Language { get; set; } = "Japanese";

        public List<JikanDotNet.CharacterEntry> CharactersList { get; set; } = new();

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

            if (string.IsNullOrWhiteSpace(CurrentAnime.MyAnimeListId))
            {
                NavigationManager.NavigateTo("/animes");
                return;
            }

            StateHasChanged();

            await _LoadCharacters();

            StateHasChanged();
        }

        protected void _Character_OnClicked(JikanDotNet.CharacterEntry character)
        {
            var voiceActor = character?.VoiceActors?.FirstOrDefault(va => va.Language == Language);

            if (voiceActor == null) { return; }

            NavigationManager.NavigateTo($"/characters/{voiceActor.MalId}");
        }

        async Task _LoadCharacters()
        {
            var animeCharactersStaff = await Jikan.GetAnimeCharactersStaff(long.Parse(CurrentAnime.MyAnimeListId));
            CharactersList = animeCharactersStaff.Characters.ToList();
        }
    }
}
