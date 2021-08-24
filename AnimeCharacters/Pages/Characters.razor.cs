using AnimeCharacters.Models;
using Kitsu.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeCharacters.Pages
{
    public partial class Characters
    {
        [Inject]
        AniListClient.AniListClient _anilistClient { get; set; }

        public User CurrentUser { get; set; }

        public AniListClient.Models.Staff CurrentPerson { get; set; }

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
            CurrentPerson = await _anilistClient.Staff.GetStaffById(int.Parse(Id));

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
            var vaRoles = new Dictionary<string, AniListClient.Models.Character>();

            foreach(var person in CurrentPerson.Characters.Where(role => role.Media != null))
            {
                foreach(var mediaItem in person.Media)
                {
                    // TODO: Allow VAs to have multiple roles in the same anime
                    vaRoles.TryAdd(mediaItem.Id.ToString(), person);
                }
            }

            var libraryEntries = await DatabaseProvider.GetLibrariesAsync();

            MyCharactersList =
                libraryEntries.Where(libraryEntry => !string.IsNullOrWhiteSpace(libraryEntry.Anime.AnilistId) && vaRoles.ContainsKey(libraryEntry.Anime.AnilistId))
                              .Select(libraryEntry => new CharacterAnimeModel
                              {
                                  KitsuId = libraryEntry.Anime.KitsuId,
                                  AnimeImageUrl = libraryEntry.Anime.PosterImageUrl,
                                  LastProgressedAt = libraryEntry.ProgressedAt,
                                  VoiceActingRole = vaRoles[libraryEntry.Anime.AnilistId],
                              })
                              .OrderByDescending(item => item.LastProgressedAt ?? System.DateTimeOffset.MinValue)
                              .ToList();

        }
    }
}
