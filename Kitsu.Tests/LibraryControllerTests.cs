using Kitsu.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Kitsu.Tests
{
    [TestClass]
    public class LibraryControllerTests
    {
        [TestMethod]
        public async Task GetLibraryCollectionByIdsAsync_MoreThanTenEntries_FetchesEveryChunkWithAnimeMetadata()
        {
            using var handler = new LibraryHandler();
            using var client = new HttpClient(handler);
            using var controller = new LibraryController("https://example.com/api/edge/", client);
            var ids = Enumerable.Range(1, 21).Select(id => (long)id).ToArray();
            var results = await controller.GetLibraryCollectionByIdsAsync(1, ids, LibraryType.Anime);

            Assert.AreEqual(3, handler.RequestCount);
            CollectionAssert.AreEquivalent(ids, results.Select(entry => entry.Id).ToArray());
            Assert.IsTrue(results.All(entry => entry.Anime.Title == $"Anime {entry.Id}"
                && entry.Anime.MetadataFetchedAt.HasValue));
        }

        [TestMethod]
        public async Task GetCompleteLibraryCollectionAsync_MappingAndAnimeShareId_UsesAnimeResource()
        {
            using var handler = new LibraryHandler();
            using var client = new HttpClient(handler);
            using var controller = new LibraryController("https://example.com/api/edge/", client);
            var results = await controller.GetCompleteLibraryCollectionAsync(1, LibraryType.Anime);

            Assert.AreEqual("Anime 1", results.Single().Anime.Title);
            Assert.AreEqual("https://example.com/1.jpg", results.Single().Anime.PosterImageUrl);
        }

        sealed class LibraryHandler : HttpMessageHandler
        {
            public int RequestCount { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
            {
                RequestCount++;
                var query = HttpUtility.ParseQueryString(request.RequestUri.Query);
                var ids = (query["filter[id]"] ?? "1").Split(',');
                Assert.IsTrue(ids.Length <= 10);
                Assert.AreEqual("anime", query["filter[kind]"]);
                var json = JsonSerializer.Serialize(new
                {
                    data = ids.Select(id => new
                    {
                        id,
                        type = "libraryEntries",
                        attributes = new { status = "current", reconsuming = false },
                        relationships = new { anime = new { data = new { id, type = "anime" } } }
                    }),
                    included = ids.SelectMany(id => new object[]
                    {
                        new { id, type = "mappings", attributes = new { externalSite = "anilist/anime", externalId = "42" } },
                        new
                        {
                            id,
                            type = "anime",
                            attributes = new
                            {
                                canonicalTitle = $"Anime {id}",
                                titles = new { en = $"Anime {id}" },
                                posterImage = new { tiny = $"https://example.com/{id}.jpg" }
                            },
                            relationships = new { mappings = new { data = Array.Empty<object>() } }
                        }
                    }),
                    links = new { next = (string)null }
                });
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) });
            }
        }
    }
}
