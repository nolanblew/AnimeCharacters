using AniListClient.Enums;
using AniListClient.Models;
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

        public Language Language { get; set; } = Language.Japanese;

        public List<AniListClient.Models.Character> CharactersList { get; set; } = new();

        [Parameter]
        public string Id { get; set; }

        bool _IsLoading { get; set; } = true;

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

            _IsLoading = true;
            StateHasChanged();

            await _LoadCharacters();

            _IsLoading = false;
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
            var media = await AnilistClient.Characters.GetMediaWithCharactersById(int.Parse(CurrentAnime.AnilistId), Language);
            CharactersList = media.Characters.OrderByDescending(c => c, CharacterByRoleComparer.Instance).ToList();
        }

        async Task _SelectLanguage(ChangeEventArgs e)
        {
            Language = ((string)e.Value) switch
            {
                "Japanese" => Language.Japanese,
                "English" => Language.English,
                "Korean" => Language.Korean,
                "Chinese" => Language.Chinese,
                "Portugese" => Language.Portuguese,
                "Spanish" => Language.Spanish,
                _ => Language.Japanese
            };


            _IsLoading = true;
            StateHasChanged();

            await _LoadCharacters();

            _IsLoading = false;
            StateHasChanged();
        }
    }
}
