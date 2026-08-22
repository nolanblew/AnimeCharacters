using Kitsu.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReferenceApis;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Kitsu.Tests.ReferenceApis
{
    [TestClass]
    public class JikanReferenceAnimeProviderTests
    {
        [TestMethod]
        public async Task GetMediaWithCharactersAsync_MapsJapaneseVoiceActorsFromMyAnimeList()
        {
            var provider = new JikanReferenceAnimeProvider(new HttpClient(new StubHttpMessageHandler(request =>
            {
                Assert.AreEqual("https://api.jikan.moe/v4/anime/1/characters", request.RequestUri.ToString());

                return JsonResponse("""
                {
                  "data": [
                    {
                      "character": {
                        "mal_id": 1,
                        "name": "Spiegel, Spike",
                        "images": { "jpg": { "image_url": "https://cdn.example/spike.jpg" } }
                      },
                      "role": "Main",
                      "voice_actors": [
                        {
                          "person": {
                            "mal_id": 11,
                            "url": "https://myanimelist.net/people/11/Kouichi_Yamadera",
                            "name": "Yamadera, Kouichi"
                          },
                          "language": "Japanese"
                        },
                        {
                          "person": {
                            "mal_id": 12,
                            "url": "https://myanimelist.net/people/12/Steven_Blum",
                            "name": "Blum, Steven"
                          },
                          "language": "English"
                        }
                      ]
                    }
                  ]
                }
                """);
            })));

            var anime = new Anime
            {
                KitsuId = "cowboy-bebop",
                MyAnimeListId = "1",
                Title = "Cowboy Bebop",
                RomanjiTitle = "Cowboy Bebop",
                PosterImageUrl = "https://cdn.example/cowboy-bebop.jpg"
            };

            var result = await provider.GetMediaWithCharactersAsync(anime, new[] { "Cowboy Bebop" });

            Assert.AreEqual(ReferenceProviderNames.Jikan, result.AnimeKey.ProviderName);
            Assert.AreEqual("1", result.AnimeKey.Id);
            Assert.AreEqual(ReferenceProviderNames.Jikan, result.Media.ProviderName);
            Assert.AreEqual("Cowboy Bebop", result.Media.Title.UserPreferred);
            Assert.AreEqual(1, result.Media.Characters.Count);
            Assert.AreEqual("Spike Spiegel", result.Media.Characters[0].Name.Full);
            Assert.AreEqual(ReferenceProviderNames.Jikan, result.Media.Characters[0].ProviderName);
            Assert.AreEqual(1, result.Media.Characters[0].VoiceActors.Count);
            Assert.AreEqual(11, result.Media.Characters[0].VoiceActors[0].Id);
            Assert.AreEqual("Kouichi Yamadera", result.Media.Characters[0].VoiceActors[0].Name.Full);
            Assert.AreEqual(ReferenceProviderNames.Jikan, result.Media.Characters[0].VoiceActors[0].ProviderName);
        }

        [TestMethod]
        public async Task GetMediaWithCharactersAsync_WhenProviderIdentityAndBaseUriAreConfigured_UsesThem()
        {
            var provider = new JikanReferenceAnimeProvider(
                new HttpClient(new StubHttpMessageHandler(request =>
                {
                    Assert.AreEqual("https://api.tenrai.org/v1/anime/1/characters", request.RequestUri.ToString());
                    return JsonResponse("""{ "data": [] }""");
                })),
                baseUri: new Uri("https://api.tenrai.org/v1/"),
                providerName: ReferenceProviderNames.Tenrai,
                displayName: "Tenrai");
            var anime = new Anime { MyAnimeListId = "1", Title = "Cowboy Bebop" };

            var result = await provider.GetMediaWithCharactersAsync(anime, new[] { anime.Title });

            Assert.AreEqual(ReferenceProviderNames.Tenrai, provider.Name);
            Assert.AreEqual("Tenrai", provider.DisplayName);
            Assert.AreEqual(ReferenceProviderNames.Tenrai, result.AnimeKey.ProviderName);
            Assert.AreEqual(ReferenceProviderNames.Tenrai, result.Media.ProviderName);
        }

        [TestMethod]
        public async Task GetStaffByIdAsync_MapsPersonDetailsAndVoiceRoles()
        {
            var provider = new JikanReferenceAnimeProvider(new HttpClient(new StubHttpMessageHandler(request =>
            {
                Assert.AreEqual("https://api.jikan.moe/v4/people/11/full", request.RequestUri.ToString());

                return JsonResponse("""
                {
                  "data": {
                    "mal_id": 11,
                    "url": "https://myanimelist.net/people/11/Kouichi_Yamadera",
                    "images": { "jpg": { "image_url": "https://cdn.example/yamadera.jpg" } },
                    "name": "Kouichi Yamadera",
                    "birthday": "1961-06-17T00:00:00+00:00",
                    "about": "Blood type: A\nBirthplace: Miyagi Prefecture, Japan",
                    "voices": [
                      {
                        "role": "Main",
                        "anime": {
                          "mal_id": 1,
                          "title": "Cowboy Bebop"
                        },
                        "character": {
                          "mal_id": 1,
                          "name": "Spiegel, Spike",
                          "images": { "jpg": { "image_url": "https://cdn.example/spike.jpg" } }
                        }
                      }
                    ]
                  }
                }
                """);
            })));

            var staff = await provider.GetStaffByIdAsync("11");

            Assert.AreEqual(11, staff.Id);
            Assert.AreEqual("Kouichi Yamadera", staff.Name.Full);
            Assert.AreEqual(ReferenceProviderNames.Jikan, staff.ProviderName);
            Assert.AreEqual("A", staff.BloodType);
            Assert.AreEqual(1961, staff.DateOfBirth.Year);
            Assert.AreEqual(1, staff.Characters.Count);
            Assert.AreEqual("Spike Spiegel", staff.Characters[0].Name.Full);
            Assert.AreEqual(ReferenceProviderNames.Jikan, staff.Characters[0].ProviderName);
            Assert.AreEqual(1, staff.Characters[0].Media.Count);
            Assert.AreEqual(1, staff.Characters[0].Media[0].Id);
            Assert.AreEqual(ReferenceProviderNames.Jikan, staff.Characters[0].Media[0].ProviderName);
        }

        [TestMethod]
        public async Task GetStaffByIdAsync_WhenRequestStalls_UsesProviderTimeout()
        {
            var provider = new JikanReferenceAnimeProvider(
                new HttpClient(new NeverCompletingHttpMessageHandler()),
                TimeSpan.FromMilliseconds(20));

            var exception = await Assert.ThrowsExceptionAsync<ReferenceApiProviderException>(
                () => provider.GetStaffByIdAsync("37562"));

            Assert.AreEqual("Jikan request timed out.", exception.Message);
        }

        [TestMethod]
        public async Task GetMediaWithCharactersAsync_WhenJikanIsTemporarilyUnavailable_RetriesRequest()
        {
            var requestCount = 0;
            var provider = new JikanReferenceAnimeProvider(new HttpClient(new StubHttpMessageHandler(_ =>
            {
                requestCount++;

                if (requestCount == 1)
                {
                    return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                    {
                        Content = new StringContent("Temporary upstream failure")
                    };
                }

                return JsonResponse("""{ "data": [] }""");
            })));
            var anime = new Anime { MyAnimeListId = "1", Title = "Cowboy Bebop" };

            var result = await provider.GetMediaWithCharactersAsync(anime, new[] { anime.Title });

            Assert.AreEqual(2, requestCount);
            Assert.IsNotNull(result.Media);
        }

        [TestMethod]
        public async Task GetMediaWithCharactersAsync_WhenJikanRateLimits_HonorsRetryAfterAndRetries()
        {
            var requestCount = 0;
            var provider = new JikanReferenceAnimeProvider(new HttpClient(new StubHttpMessageHandler(_ =>
            {
                requestCount++;

                if (requestCount == 1)
                {
                    var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
                    response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromMilliseconds(1));
                    return response;
                }

                return JsonResponse("""{ "data": [] }""");
            })));
            var anime = new Anime { MyAnimeListId = "1", Title = "Cowboy Bebop" };

            await provider.GetMediaWithCharactersAsync(anime, new[] { anime.Title });

            Assert.AreEqual(2, requestCount);
        }

        [TestMethod]
        public async Task GetMediaWithCharactersAsync_WhenJikanReturnsInvalidJson_ThrowsProviderException()
        {
            var provider = new JikanReferenceAnimeProvider(new HttpClient(new StubHttpMessageHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("<html>Gateway error</html>", Encoding.UTF8, "text/html")
                })));
            var anime = new Anime { MyAnimeListId = "1", Title = "Cowboy Bebop" };

            var exception = await Assert.ThrowsExceptionAsync<ReferenceApiProviderException>(
                () => provider.GetMediaWithCharactersAsync(anime, new[] { anime.Title }));

            StringAssert.Contains(exception.Message, "invalid response");
            Assert.IsInstanceOfType<JsonException>(exception.InnerException);
        }

        [TestMethod]
        public async Task GetMediaWithCharactersAsync_WhenJikanRemainsUnavailable_StopsAfterThreeAttempts()
        {
            var requestCount = 0;
            var provider = new JikanReferenceAnimeProvider(new HttpClient(new StubHttpMessageHandler(_ =>
            {
                requestCount++;
                return new HttpResponseMessage(HttpStatusCode.BadGateway);
            })));
            var anime = new Anime { MyAnimeListId = "1", Title = "Cowboy Bebop" };

            var exception = await Assert.ThrowsExceptionAsync<ReferenceApiProviderException>(
                () => provider.GetMediaWithCharactersAsync(anime, new[] { anime.Title }));

            Assert.AreEqual(3, requestCount);
            StringAssert.Contains(exception.Message, "HTTP 502");
        }

        [TestMethod]
        public async Task GetMediaWithCharactersAsync_WhenNetworkRemainsUnavailable_StopsAfterThreeAttempts()
        {
            var requestCount = 0;
            var provider = new JikanReferenceAnimeProvider(new HttpClient(new StubHttpMessageHandler(_ =>
            {
                requestCount++;
                throw new HttpRequestException("Network unavailable");
            })));
            var anime = new Anime { MyAnimeListId = "1", Title = "Cowboy Bebop" };

            var exception = await Assert.ThrowsExceptionAsync<ReferenceApiProviderException>(
                () => provider.GetMediaWithCharactersAsync(anime, new[] { anime.Title }));

            Assert.AreEqual(3, requestCount);
            StringAssert.Contains(exception.Message, "after retrying");
            Assert.IsInstanceOfType<HttpRequestException>(exception.InnerException);
        }

        [TestMethod]
        public async Task GetMediaWithCharactersAsync_WhenJikanReturnsPermanentFailure_DoesNotRetry()
        {
            var requestCount = 0;
            var provider = new JikanReferenceAnimeProvider(new HttpClient(new StubHttpMessageHandler(_ =>
            {
                requestCount++;
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            })));
            var anime = new Anime { MyAnimeListId = "1", Title = "Cowboy Bebop" };

            var exception = await Assert.ThrowsExceptionAsync<ReferenceApiProviderException>(
                () => provider.GetMediaWithCharactersAsync(anime, new[] { anime.Title }));

            Assert.AreEqual(1, requestCount);
            StringAssert.Contains(exception.Message, "HTTP 404");
        }

        [TestMethod]
        public async Task GetMediaWithCharactersAsync_WhenTenraiFails_UsesTenraiInDiagnostic()
        {
            var provider = new JikanReferenceAnimeProvider(
                new HttpClient(new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound))),
                baseUri: new Uri("https://api.tenrai.org/v1/"),
                providerName: ReferenceProviderNames.Tenrai,
                displayName: "Tenrai");
            var anime = new Anime { MyAnimeListId = "1", Title = "Cowboy Bebop" };

            var exception = await Assert.ThrowsExceptionAsync<ReferenceApiProviderException>(
                () => provider.GetMediaWithCharactersAsync(anime, new[] { anime.Title }));

            StringAssert.StartsWith(exception.Message, "Tenrai returned HTTP 404");
        }

        static HttpResponseMessage JsonResponse(string json) =>
            new(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

        class StubHttpMessageHandler : HttpMessageHandler
        {
            readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

            public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
            {
                _responseFactory = responseFactory;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
                Task.FromResult(_responseFactory(request));
        }

        class NeverCompletingHttpMessageHandler : HttpMessageHandler
        {
            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                throw new InvalidOperationException("The cancellation token should stop this request.");
            }
        }
    }
}
