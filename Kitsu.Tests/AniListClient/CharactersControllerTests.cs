using AniListClient.Controllers;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Kitsu.Tests.AniListClient
{
    [TestClass]
    public class CharactersControllerTests
    {
        [TestMethod]
        public async Task GetMediaWithCharactersById_WhenAniListReturnsGraphQLHttpError_ThrowsReadableMessage()
        {
            using var graphQLClient = new GraphQLHttpClient(
                new GraphQLHttpClientOptions
                {
                    EndPoint = new Uri("https://graphql.anilist.co"),
                    HttpMessageHandler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.Forbidden)
                    {
                        Content = new StringContent("""
                        {
                            "errors": [
                                {
                                    "message": "The AniList API has been temporarily disabled due to severe stability issues.",
                                    "status": 403
                                }
                            ],
                            "data": null
                        }
                        """)
                    })
                },
                new NewtonsoftJsonSerializer());
            var controller = new CharactersController(graphQLClient);

            var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => controller.GetMediaWithCharactersById(123));

            StringAssert.Contains(exception.Message, "temporarily disabled");
        }

        [TestMethod]
        public async Task GetMediaWithCharactersById_WhenAniListCannotBeReached_ThrowsReadableMessage()
        {
            using var graphQLClient = new GraphQLHttpClient(
                new GraphQLHttpClientOptions
                {
                    EndPoint = new Uri("https://graphql.anilist.co"),
                    HttpMessageHandler = new StubHttpMessageHandler(new HttpRequestException("Network failure"))
                },
                new NewtonsoftJsonSerializer());
            var controller = new CharactersController(graphQLClient);

            var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => controller.GetMediaWithCharactersById(123));

            StringAssert.Contains(exception.Message, "Unable to reach AniList");
        }

        private class StubHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;
            private readonly Exception _exception;

            public StubHttpMessageHandler(HttpResponseMessage response)
            {
                _response = response;
            }

            public StubHttpMessageHandler(Exception exception)
            {
                _exception = exception;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
                _exception != null
                    ? Task.FromException<HttpResponseMessage>(_exception)
                    : Task.FromResult(_response);
        }
    }
}
