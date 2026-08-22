using AniListClient.Models;
using Kitsu.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReferenceApis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
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

        [TestMethod]
        public async Task GetStaffByIdAsync_WhenRequestedProviderFails_UsesExactNameMatchFromAnotherProvider()
        {
            var expectedStaff = new Staff(
                Id: 123,
                Name: new Names(null, null, null, "Shouya Chiba", null, null, null),
                Language: Language.Japanese,
                Images: null,
                Description: null,
                Age: null,
                DateOfBirth: null,
                BloodType: null,
                SiteUrl: null,
                Characters: new List<Character>(),
                ProviderName: ReferenceProviderNames.AniList);
            var service = new ReferenceAnimeService(new IReferenceAnimeProvider[]
            {
                new StubProvider(
                    ReferenceProviderNames.Jikan,
                    "Jikan / MyAnimeList",
                    anime => anime.MyAnimeListId,
                    getStaffById: _ => throw new ReferenceApiProviderException("Jikan could not be reached.")),
                new StubProvider(
                    ReferenceProviderNames.AniList,
                    "AniList",
                    anime => anime.AnilistId,
                    searchStaffByName: _ => Task.FromResult<IReadOnlyList<Staff>>(new[] { expectedStaff }))
            });

            var staff = await service.GetStaffByIdAsync(
                ReferenceProviderNames.Jikan,
                "37562",
                "Shouya Chiba");

            Assert.AreSame(expectedStaff, staff);
        }

        [TestMethod]
        public async Task GetMediaWithCharactersAsync_WhenFirstProviderFails_UsesNextProvider()
        {
            var anime = new Anime { MyAnimeListId = "1", AnilistId = "5" };
            var expected = new ReferenceMediaResult(
                new ReferenceAnimeKey(ReferenceProviderNames.AniList, "5"),
                new Media(
                    5,
                    new Titles("Cowboy Bebop", null, null, "Cowboy Bebop"),
                    null,
                    null,
                    MediaStatus.Finished,
                    new List<Character>(),
                    ReferenceProviderNames.AniList));
            var service = new ReferenceAnimeService(new IReferenceAnimeProvider[]
            {
                new StubProvider(
                    ReferenceProviderNames.Jikan,
                    "Jikan",
                    item => item.MyAnimeListId,
                    getMedia: (_, _) => throw new ReferenceApiProviderException("Jikan returned an invalid response.")),
                new StubProvider(
                    ReferenceProviderNames.AniList,
                    "AniList",
                    item => item.AnilistId,
                    getMedia: (_, _) => Task.FromResult(expected))
            });

            var result = await service.GetMediaWithCharactersAsync(anime, new[] { "Cowboy Bebop" });

            Assert.AreSame(expected, result);
        }

        [TestMethod]
        public async Task GetMediaWithCharactersAsync_WhenJikanRetriesAreExhausted_UsesAniListProvider()
        {
            var anime = new Anime { MyAnimeListId = "1", AnilistId = "5" };
            var expected = new ReferenceMediaResult(
                new ReferenceAnimeKey(ReferenceProviderNames.AniList, "5"),
                new Media(
                    5,
                    new Titles("Cowboy Bebop", null, null, "Cowboy Bebop"),
                    null,
                    null,
                    MediaStatus.Finished,
                    new List<Character>(),
                    ReferenceProviderNames.AniList));
            var jikanRequestCount = 0;
            var service = new ReferenceAnimeService(new IReferenceAnimeProvider[]
            {
                new JikanReferenceAnimeProvider(new HttpClient(new StubHttpMessageHandler(() =>
                {
                    jikanRequestCount++;
                    return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                }))),
                new StubProvider(
                    ReferenceProviderNames.AniList,
                    "AniList",
                    item => item.AnilistId,
                    getMedia: (_, _) => Task.FromResult(expected))
            });

            var result = await service.GetMediaWithCharactersAsync(anime, new[] { "Cowboy Bebop" });

            Assert.AreEqual(3, jikanRequestCount);
            Assert.AreSame(expected, result);
        }

        class StubProvider : IReferenceAnimeProvider
        {
            readonly Func<Anime, string> _animeIdSelector;
            readonly Func<string, Task<Staff>> _getStaffById;
            readonly Func<string, Task<IReadOnlyList<Staff>>> _searchStaffByName;
            readonly Func<Anime, IReadOnlyCollection<string>, Task<ReferenceMediaResult>> _getMedia;

            public StubProvider(
                string name,
                string displayName,
                Func<Anime, string> animeIdSelector,
                Func<string, Task<Staff>> getStaffById = null,
                Func<string, Task<IReadOnlyList<Staff>>> searchStaffByName = null,
                Func<Anime, IReadOnlyCollection<string>, Task<ReferenceMediaResult>> getMedia = null)
            {
                Name = name;
                DisplayName = displayName;
                _animeIdSelector = animeIdSelector;
                _getStaffById = getStaffById;
                _searchStaffByName = searchStaffByName;
                _getMedia = getMedia;
            }

            public string Name { get; }
            public string DisplayName { get; }

            public ReferenceAnimeKey GetKnownAnimeKey(Anime anime)
            {
                var animeId = _animeIdSelector(anime);
                return string.IsNullOrWhiteSpace(animeId) ? null : new ReferenceAnimeKey(Name, animeId);
            }

            public Task<ReferenceMediaResult> GetMediaWithCharactersAsync(Anime anime, IReadOnlyCollection<string> searchTitles) =>
                _getMedia?.Invoke(anime, searchTitles) ?? Task.FromResult<ReferenceMediaResult>(null);

            public Task<Staff> GetStaffByIdAsync(string id) =>
                _getStaffById?.Invoke(id) ?? Task.FromResult<Staff>(null);

            public async Task<Staff> FindStaffByNameAsync(string name) =>
                (await (_searchStaffByName?.Invoke(name) ?? Task.FromResult<IReadOnlyList<Staff>>(new List<Staff>())))
                    .FirstOrDefault();
        }

        class StubHttpMessageHandler : HttpMessageHandler
        {
            readonly Func<HttpResponseMessage> _responseFactory;

            public StubHttpMessageHandler(Func<HttpResponseMessage> responseFactory)
            {
                _responseFactory = responseFactory;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
                Task.FromResult(_responseFactory());
        }
    }
}
