using AnimeCharacters.Helpers;
using AnimeCharacters.Models;
using AniListClient.Models;
using Kitsu.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeCharacters.Extensions
{
    public class KitsuLibraryCreditProvider : IVoiceActorCreditProvider
    {
        readonly IDatabaseProvider _databaseProvider;

        public KitsuLibraryCreditProvider(IDatabaseProvider databaseProvider)
        {
            _databaseProvider = databaseProvider;
        }

        public string ExtensionId => BuiltInExtensionIds.KitsuLibrary;

        public async Task<IReadOnlyList<VoiceActorCreditModel>> GetCreditsAsync(Staff staff)
        {
            if (staff?.Characters == null)
            {
                return new List<VoiceActorCreditModel>();
            }

            var vaRoles = staff.Characters
                .Where(role => role.Media != null)
                .SelectMany(character => character.Media.Select(mediaItem => new { MediaId = mediaItem.Id.ToString(), Character = character }))
                .GroupBy(role => role.MediaId)
                .ToDictionary(group => group.Key, group => group.Select(role => role.Character).ToList());

            var libraryEntries = await _databaseProvider.GetLibrariesAsync() ?? new List<LibraryEntry>();
            var settings = await _databaseProvider.GetUserSettingsAsync() ?? new UserSettings();

            return libraryEntries
                .Where(libraryEntry =>
                    !string.IsNullOrWhiteSpace(libraryEntry.Anime?.AnilistId) &&
                    vaRoles.ContainsKey(libraryEntry.Anime.AnilistId))
                .SelectMany(libraryEntry =>
                {
                    var characters = vaRoles[libraryEntry.Anime.AnilistId];
                    return characters.Select(character => CreateCredit(libraryEntry, character, settings));
                })
                .OrderByDescending(item => item.LastProgressedAt ?? DateTimeOffset.MinValue)
                .ToList();
        }

        static VoiceActorCreditModel CreateCredit(LibraryEntry libraryEntry, Character character, UserSettings settings) =>
            new()
            {
                ExtensionId = BuiltInExtensionIds.KitsuLibrary,
                ExtensionName = "Kitsu Library",
                CategoryName = "Anime",
                MediaId = libraryEntry.Anime.KitsuId,
                MediaTitle = GetMediaTitle(libraryEntry, character, settings),
                MediaImageUrl = libraryEntry.Anime.PosterImageUrl,
                MediaRoute = $"/animes/{libraryEntry.Anime.KitsuId}",
                CharacterId = character.Id.ToString(),
                CharacterName = character.Name?.Full,
                CharacterImageUrl = character.Image?.GetOptimalImage(),
                LastProgressedAt = libraryEntry.ProgressedAt
            };

        static string GetMediaTitle(LibraryEntry libraryEntry, Character character, UserSettings settings)
        {
            var media = character.Media?.FirstOrDefault();

            return media != null
                ? TitleHelper.GetPreferredTitle(media, settings.PreferredTitleType)
                : libraryEntry.Anime.EnglishTitle ?? libraryEntry.Anime.Title;
        }
    }
}
