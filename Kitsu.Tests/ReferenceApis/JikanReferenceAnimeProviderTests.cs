using Kitsu.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReferenceApis;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
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
    }
}
