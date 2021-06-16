using AnimeCharacters.Models;
using Kitsu.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeCharacters.Pages
{
    public partial class Characters
    {
        [Inject]
        JikanDotNet.IJikan Jikan { get; set; }

        public User CurrentUser { get; set; }

        public JikanDotNet.Person CurrentPerson { get; set; }

        public List<CharacterAnimeModel> MyCharactersList { get; set; } = new();
        public List<CharacterAnimeModel> NotMyCharactersList { get; set; } = new();

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
            CurrentPerson = await Jikan.GetPerson(long.Parse(Id));

            if (CurrentPerson == null)
            {
                NavigationManager.NavigateTo("/animes");
                return;
            }

            StateHasChanged();

            await _LoadCharacters();

            StateHasChanged();
        }

        protected void _OnAnimeClicked(CharacterAnimeModel model)
        {
            if (model == null) { return; }

            NavigationManager.NavigateTo($"/animes/{model.KitsuId}");
        }

        protected void _OnCharacterClicked(CharacterAnimeModel model)
        {
        }

        async Task _LoadCharacters()
        {
            var vaRoles = CurrentPerson
                .VoiceActingRoles
                .GroupBy(role => role.Anime.MalId)
                .ToDictionary(group => group.Key.ToString());

            var libraryEntries = await DatabaseProvider.GetLibrariesAsync();

            MyCharactersList =
                libraryEntries.Where(libraryEntry => !string.IsNullOrWhiteSpace(libraryEntry.Anime.MyAnimeListId) && vaRoles.ContainsKey(libraryEntry.Anime.MyAnimeListId))
                              .Select(libraryEntry => new CharacterAnimeModel
                              {
                                  KitsuId = libraryEntry.Anime.KitsuId,
                                  AnimeImageUrl = libraryEntry.Anime.PosterImageUrl,
                                  LastProgressedAt = libraryEntry.ProgressedAt,
                                  VoiceActingRole = vaRoles[libraryEntry.Anime.MyAnimeListId].FirstOrDefault(),
                              })
                              .OrderByDescending(item => item.LastProgressedAt.Value)
                              .ToList();

        }
    }
}
