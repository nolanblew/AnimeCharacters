using AniListClient.Models;
using Kitsu.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReferenceApis
{
    public class AniListReferenceAnimeProvider : IReferenceAnimeProvider
    {
        readonly AniListClient.AniListClient _aniListClient;

        public AniListReferenceAnimeProvider(AniListClient.AniListClient aniListClient)
        {
            _aniListClient = aniListClient;
        }

        public string Name => ReferenceProviderNames.AniList;
        public string DisplayName => "AniList";

        public ReferenceAnimeKey GetKnownAnimeKey(Anime anime) =>
            !string.IsNullOrWhiteSpace(anime?.AnilistId)
                ? new ReferenceAnimeKey(Name, anime.AnilistId)
                : null;

        public async Task<ReferenceMediaResult> GetMediaWithCharactersAsync(
            Anime anime,
            IReadOnlyCollection<string> searchTitles)
        {
            var animeId = anime?.AnilistId;

            if (string.IsNullOrWhiteSpace(animeId))
            {
                animeId = await SearchAnimeIdAsync(searchTitles);
            }

            if (string.IsNullOrWhiteSpace(animeId) || !int.TryParse(animeId, out var id))
            {
                throw new ReferenceApiProviderException("AniList does not have a matching anime id.");
            }

            var media = await _aniListClient.Characters.GetMediaWithCharactersById(id);
            return new ReferenceMediaResult(new ReferenceAnimeKey(Name, animeId), media);
        }

        public async Task<Staff> GetStaffByIdAsync(string id)
        {
            if (!int.TryParse(id, out var staffId))
            {
                throw new ReferenceApiProviderException("AniList staff ids must be numeric.");
            }

            return await _aniListClient.Staff.GetStaffById(staffId);
        }

        public async Task<Staff> FindStaffByNameAsync(string name)
        {
            var matches = await _aniListClient.Staff.SearchStaffByName(name);
            var match = matches.FirstOrDefault(staff => StaffNameMatcher.IsExactMatch(staff?.Name, name));

            return match == null
                ? null
                : await _aniListClient.Staff.GetStaffById(match.Id);
        }

        async Task<string> SearchAnimeIdAsync(IReadOnlyCollection<string> searchTitles)
        {
            foreach (var title in searchTitles ?? Array.Empty<string>())
            {
                var media = await _aniListClient.Characters.SearchMediaByTitle(title);

                if (media == null || !_IsTitleMatch(media.Title, searchTitles))
                {
                    continue;
                }

                return media.Id.ToString();
            }

            return null;
        }

        static bool _IsTitleMatch(Titles mediaTitle, IEnumerable<string> searchTitles)
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
    }
}
