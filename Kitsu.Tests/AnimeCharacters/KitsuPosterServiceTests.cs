using AnimeCharacters.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Kitsu.Tests.AnimeCharacters
{
    [TestClass]
    public class KitsuPosterServiceTests
    {
        [TestMethod]
        public async Task GetPosterUrlsAsync_OriginalOnlyResponse_RenewsExactAnimeAndSharesRequests()
        {
            var release = new TaskCompletionSource<HttpResponseMessage>();
            using var handler = new PosterHandler(_ => release.Task);
            using var httpClient = new HttpClient(handler);
            var service = CreateService(httpClient);
            var first = service.GetPosterUrlsAsync("50021");
            var second = service.GetPosterUrlsAsync("50021");

            Assert.AreEqual(1, handler.RequestCount);
            Assert.AreEqual("/api/edge/anime/50021", handler.RequestUri.AbsolutePath);
            Assert.AreEqual("application/vnd.api+json", handler.Accept);
            var renewed = $"https://example.com/poster.jpg?X-Amz-Date={DateTimeOffset.UtcNow:yyyyMMddTHHmmssZ}&X-Amz-Expires=900";
            release.SetResult(Response(System.Text.Json.JsonSerializer.Serialize(
                new { data = new { attributes = new { posterImage = new { original = renewed } } } })));

            CollectionAssert.AreEqual(new[] { renewed }, (await first).ToArray());
            CollectionAssert.AreEqual((await first).ToArray(), (await second).ToArray());
        }

        [TestMethod]
        public async Task GetPosterUrlsAsync_MultipleSizes_ProvidesDistinctAlternativesAndSkipsExpiredLinks()
        {
            using var handler = new PosterHandler(_ => Task.FromResult(Response("""
                {"data":{"attributes":{"posterImage":{
                  "large":"https://example.com/large.jpg",
                  "medium":"https://example.com/large.jpg",
                  "original":"https://example.com/old.jpg?X-Amz-Date=20200101T000000Z&X-Amz-Expires=900",
                  "small":"https://example.com/small.jpg",
                  "tiny":"https://example.com/tiny.jpg"}}}}
                """)));
            using var httpClient = new HttpClient(handler);

            CollectionAssert.AreEqual(new[] { "https://example.com/large.jpg", "https://example.com/small.jpg",
                "https://example.com/tiny.jpg" }, (await CreateService(httpClient).GetPosterUrlsAsync("1")).ToArray());
        }

        [DataTestMethod]
        [DataRow("{\"data\":{\"attributes\":{\"posterImage\":null}}}", 200)]
        [DataRow("{\"data\":{\"attributes\":{\"posterImage\":{}}}}", 200)]
        [DataRow("not json", 200)]
        [DataRow("unavailable", 503)]
        public async Task GetPosterUrlsAsync_MissingPosterOrApiFailure_ReturnsNoCandidates(string json, int status)
        {
            using var handler = new PosterHandler(_ => Task.FromResult(Response(json, (HttpStatusCode)status)));
            using var httpClient = new HttpClient(handler);

            Assert.AreEqual(0, (await CreateService(httpClient).GetPosterUrlsAsync("1")).Count);
        }

        [TestMethod]
        public async Task GetPosterUrlsAsync_Timeout_ReturnsNoCandidates()
        {
            using var handler = new PosterHandler(_ => throw new TaskCanceledException());
            using var httpClient = new HttpClient(handler);
            Assert.AreEqual(0, (await CreateService(httpClient).GetPosterUrlsAsync("1")).Count);
        }

        [TestMethod]
        public async Task GetPosterUrlsAsync_InvalidId_DoesNotCallApi()
        {
            using var handler = new PosterHandler(_ => throw new AssertFailedException());
            using var httpClient = new HttpClient(handler);
            Assert.AreEqual(0, (await CreateService(httpClient).GetPosterUrlsAsync("../users")).Count);
        }

        static KitsuPosterService CreateService(HttpClient client) => new(client, NullLogger<KitsuPosterService>.Instance);

        static HttpResponseMessage Response(string json, HttpStatusCode status = HttpStatusCode.OK) =>
            new(status) { Content = new StringContent(json) };

        sealed class PosterHandler : HttpMessageHandler
        {
            readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _respond;
            public int RequestCount { get; private set; }
            public Uri RequestUri { get; private set; }
            public string Accept { get; private set; }

            public PosterHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond) => _respond = respond;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
            {
                RequestCount++;
                RequestUri = request.RequestUri;
                Accept = request.Headers.Accept.ToString();
                return _respond(request);
            }
        }
    }
}
