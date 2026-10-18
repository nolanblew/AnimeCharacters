using AnimeCharacters.Components;
using AnimeCharacters.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable BL0006 // This test renderer dispatches the image's actual Blazor error events.

namespace Kitsu.Tests.AnimeCharacters
{
    [TestClass]
    public class RemoteImageTests
    {
        [TestMethod]
        public async Task ExpiredPoster_RenewsBeforeRenderingExpiredUrl()
        {
            using var handler = new ImageHandler();
            using var renderer = CreateRenderer(handler);
            await renderer.RenderAsync(
                "https://example.com/expired.jpg?X-Amz-Date=20200101T000000Z&X-Amz-Expires=900", "50021");

            Assert.AreEqual("https://example.com/large.jpg", renderer.Source);
            Assert.AreEqual(1, handler.RequestCount);
        }

        [TestMethod]
        public async Task BrokenPoster_TriesCurrentSizesThenPlaceholderWithoutLooping()
        {
            using var handler = new ImageHandler();
            using var renderer = CreateRenderer(handler);
            await renderer.RenderAsync("https://example.com/old.jpg", "50021");
            await renderer.ImageErrorAsync();
            Assert.AreEqual("https://example.com/large.jpg", renderer.Source);
            await renderer.ImageErrorAsync();
            Assert.AreEqual("https://example.com/small.jpg", renderer.Source);
            await renderer.ImageErrorAsync();
            Assert.AreEqual("images/image-placeholder.svg", renderer.Source);
            Assert.IsFalse(renderer.HasErrorHandler);
            Assert.AreEqual(1, handler.RequestCount);
        }

        [TestMethod]
        public async Task BrokenCharacter_ShowsPlaceholderAndResetsWhenSourceChanges()
        {
            using var handler = new ImageHandler();
            using var renderer = CreateRenderer(handler);
            await renderer.RenderAsync("https://example.com/character.jpg");
            await renderer.ImageErrorAsync();
            Assert.AreEqual("images/image-placeholder.svg", renderer.Source);
            Assert.AreEqual(0, handler.RequestCount);

            await renderer.RenderAsync("https://example.com/new-character.jpg");
            Assert.AreEqual("https://example.com/new-character.jpg", renderer.Source);
        }

        [TestMethod]
        public async Task PosterRenewalFailure_ShowsPlaceholder()
        {
            using var handler = new ImageHandler { StatusCode = HttpStatusCode.ServiceUnavailable };
            using var renderer = CreateRenderer(handler);
            await renderer.RenderAsync("https://example.com/old.jpg", "50021");
            await renderer.ImageErrorAsync();
            Assert.AreEqual("images/image-placeholder.svg", renderer.Source);
            Assert.IsFalse(renderer.HasErrorHandler);
        }

        static ImageRenderer CreateRenderer(ImageHandler handler)
        {
            var services = new ServiceCollection()
                .AddSingleton(new KitsuPosterService(new HttpClient(handler), NullLogger<KitsuPosterService>.Instance))
                .BuildServiceProvider();
            return new ImageRenderer(services);
        }

        sealed class ImageRenderer : Renderer
        {
            readonly int _componentId;
            public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();

            public ImageRenderer(IServiceProvider services) : base(services, NullLoggerFactory.Instance)
            {
                _componentId = AssignRootComponentId(InstantiateComponent(typeof(RemoteImage)));
            }

            IEnumerable<RenderTreeFrame> Attributes
            {
                get
                {
                    var frames = GetCurrentRenderTreeFrames(_componentId);
                    return frames.Array.Take(frames.Count).Where(frame => frame.FrameType == RenderTreeFrameType.Attribute);
                }
            }

            public string Source => (string)Attributes.Single(frame => frame.AttributeName == "src").AttributeValue;
            public bool HasErrorHandler => Attributes.Any(frame => frame.AttributeName == "onerror");

            public Task RenderAsync(string source, string kitsuId = null) => Dispatcher.InvokeAsync(() =>
                RenderRootComponentAsync(_componentId, ParameterView.FromDictionary(new Dictionary<string, object>
                {
                    [nameof(RemoteImage.Src)] = source,
                    [nameof(RemoteImage.KitsuId)] = kitsuId,
                    [nameof(RemoteImage.Alt)] = "Example anime"
                })));

            public Task ImageErrorAsync() => Dispatcher.InvokeAsync(() => DispatchEventAsync(
                Attributes.Single(frame => frame.AttributeName == "onerror").AttributeEventHandlerId, null, EventArgs.Empty));

            protected override Task UpdateDisplayAsync(in RenderBatch renderBatch) => Task.CompletedTask;
            protected override void HandleException(Exception exception) => throw new AssertFailedException(exception.ToString());
        }

        sealed class ImageHandler : HttpMessageHandler
        {
            public int RequestCount { get; private set; }
            public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
            {
                RequestCount++;
                return Task.FromResult(new HttpResponseMessage(StatusCode)
                {
                    Content = new StringContent("""
                        {"data":{"attributes":{"posterImage":{
                        "large":"https://example.com/large.jpg","small":"https://example.com/small.jpg"}}}}
                        """)
                });
            }
        }
    }
}
