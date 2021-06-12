using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace Kitsu
{
    public static class Extensions
    {
        public static bool HasAuthenticated(this IRestClient client)
        {
            return client.Authenticator != null
                && client.Authenticator is OAuth2AuthorizationRequestHeaderAuthenticator;
        }

        public static IEnumerable<T> MaskToList<T>(this T mask, bool includeFirst = true) where T : IConvertible
        {
            var val = mask as Enum;
            Debug.Assert(val != null, nameof(val) + " != null");

            if (typeof(T).IsSubclassOf(typeof(Enum)) == false)
            { throw new ArgumentException(); }

            return Enum.GetValues(typeof(T))
                .Cast<Enum>()
                .Where(m => val.HasFlag(m))
                .Skip(includeFirst ? 0 : 1)
                .Cast<T>();
        }

        public static void AddHeader(this HttpRequestMessage requestMessage, string key, string value)
            => requestMessage.Headers.Add(key, value);

        public static void AddQueryParameter(
            this HttpRequestMessage requestMessage,
                string name,
                string value,
                bool encode = true)
        {
            if (encode)
            {
                value = HttpUtility.UrlEncode(value);
            }

            var currentUri = requestMessage.RequestUri.AbsoluteUri;

            if (currentUri.Contains("?"))
            {
                currentUri = $"{currentUri}&{name}={value}";
            } else
            {
                currentUri = $"{currentUri}?{name}={value}";
            }

            requestMessage.RequestUri = new Uri(currentUri);
        }
    }
}
