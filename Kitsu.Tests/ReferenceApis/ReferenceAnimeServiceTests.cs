using AniListClient.Models;
using Kitsu.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReferenceApis;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kitsu.Tests.ReferenceApis
{
    [TestClass]
    public class ReferenceAnimeServiceTests
    {
        [TestMethod]
        public void GetKnownAnimeKeys_ReturnsAllProviderKeysForLibraryMatching()
        {
            var anime = new Anime
            {
                MyAnimeListId = "1",
                AnilistId = "5"
            };
            var service = new ReferenceAnimeService(new IReferenceAnimeProvider[]
            {
                new StubProvider(ReferenceProviderNames.Jikan, "Jikan / MyAnimeList", anime => anime.MyAnimeListId),
                new StubProvider(ReferenceProviderNames.AniList, "AniList", anime => anime.AnilistId)
            });

            var keys = service.GetKnownAnimeKeys(anime).Select(key => key.CacheKey).ToList();

            CollectionAssert.AreEquivalent(
                new[] { "jikan:1", "anilist:5" },
                keys);
        }

        class StubProvider : IReferenceAnimeProvider
        {
            readonly System.Func<Anime, string> _animeIdSelector;

            public StubProvider(string name, string displayName, System.Func<Anime, string> animeIdSelector)
            {
                Name = name;
                DisplayName = displayName;
                _animeIdSelector = animeIdSelector;
            }

            public string Name { get; }
            public string DisplayName { get; }

            public ReferenceAnimeKey GetKnownAnimeKey(Anime anime)
            {
                var animeId = _animeIdSelector(anime);
                return string.IsNullOrWhiteSpace(animeId) ? null : new ReferenceAnimeKey(Name, animeId);
            }

            public Task<ReferenceMediaResult> GetMediaWithCharactersAsync(Anime anime, IReadOnlyCollection<string> searchTitles) =>
                Task.FromResult<ReferenceMediaResult>(null);

            public Task<Staff> GetStaffByIdAsync(string id) =>
                Task.FromResult<Staff>(null);
        }
    }
}
