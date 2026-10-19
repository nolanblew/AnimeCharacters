using AnimeCharacters.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Kitsu.Tests.AnimeCharacters
{
    [TestClass]
    public class ImageUrlHelperTests
    {
        static readonly DateTimeOffset SignedAt = new(2026, 10, 18, 12, 0, 0, TimeSpan.Zero);
        const string SignedUrl = "https://example.com/poster.jpg?X-Amz-Date=20261018T120000Z&X-Amz-Expires=900&X-Amz-Signature=example";

        [TestMethod]
        public void NeedsRefresh_OriginalOnlyKitsuLink_ExpiresAfterFifteenMinutes()
        {
            Assert.IsFalse(ImageUrlHelper.NeedsRefresh(SignedUrl, SignedAt.AddMinutes(10)));
            Assert.IsTrue(ImageUrlHelper.NeedsRefresh(SignedUrl, SignedAt.AddMinutes(14)));
            Assert.IsTrue(ImageUrlHelper.NeedsRefresh(SignedUrl, SignedAt.AddDays(1)));
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("https://media.kitsu.app/anime/1/poster_image/large.jpg")]
        [DataRow("images/cover.png")]
        [DataRow("https://example.com/poster?X-Amz-Date=invalid&X-Amz-Expires=900")]
        [DataRow("https://example.com/poster?X-Amz-Date=20261018T120000Z&X-Amz-Expires=invalid")]
        public void NeedsRefresh_UnsignedOrMalformedLink_DoesNotForceRenewal(string url)
        {
            Assert.IsFalse(ImageUrlHelper.NeedsRefresh(url, SignedAt.AddDays(1)));
        }
    }
}
