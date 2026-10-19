using System;
using System.Globalization;
using System.Web;

namespace AnimeCharacters.Helpers
{
    /// <summary>Detects expiring signed image links before the browser requests them.</summary>
    public static class ImageUrlHelper
    {
        /// <summary>Returns whether an AWS-compatible signed URL expires within the next minute.</summary>
        public static bool NeedsRefresh(string url, DateTimeOffset now)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return false;
            }

            var query = HttpUtility.ParseQueryString(uri.Query);
            if (!DateTimeOffset.TryParseExact(query["X-Amz-Date"], "yyyyMMdd'T'HHmmss'Z'",
                    CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var signedAt)
                || !double.TryParse(query["X-Amz-Expires"], NumberStyles.None,
                    CultureInfo.InvariantCulture, out var seconds))
            {
                return false;
            }

            return (now - signedAt).TotalSeconds >= seconds - 60;
        }
    }
}
