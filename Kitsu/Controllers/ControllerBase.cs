using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Kitsu.Controllers
{
    public abstract class ControllerBase : IDisposable
    {
        public ControllerBase(string baseUrl, string controllerRoute)
        {
            _baseUrl = new Uri(new Uri(baseUrl), controllerRoute);
        }

        protected HttpClient HttpClient { get; } = new();

        protected readonly string _controllerRoute;

        protected readonly Uri _baseUrl;

        public void Dispose()
        {
            HttpClient?.Dispose();
        }

        protected HttpRequestMessage _GetBaseRequest(params string[] extraParams)
        {
            var url = _baseUrl.AbsoluteUri;
            var combinedParams = string.Join("/", extraParams) ?? string.Empty;

            if (!string.IsNullOrEmpty(combinedParams)) { combinedParams = "/" + combinedParams; }
            return GetBaseRequest(url + combinedParams);
        }

        protected static HttpRequestMessage GetBaseRequest(string absolutePath)
        {
            var restRequest = new HttpRequestMessage();
            restRequest.RequestUri = new Uri(absolutePath);
            restRequest.AddHeader("Accept", "application/vnd.api+json");

            if (!restRequest.RequestUri.AbsoluteUri.Contains("page[limit]="))
            {
                restRequest.AddQueryParameter("page[limit]", "20", false);
            }

            return restRequest;
        }

        protected async Task<string> ExecuteGetRequestAsync(HttpRequestMessage requestMessage)
        {
            if (requestMessage == null) { throw new NullReferenceException("Parameter 'requestMessage' is null."); }

            var result = await HttpClient.SendAsync(requestMessage);

            if (!result.IsSuccessStatusCode) { return null; }

            var json = await result.Content.ReadAsStringAsync();

            return json;
        }

        protected async Task<T> ExecuteGetRequestAsync<T>(HttpRequestMessage requestMessage)
        {
            if (requestMessage == null) { throw new NullReferenceException("Parameter 'requestMessage' is null."); }

            var json = await ExecuteGetRequestAsync(requestMessage);

            if (string.IsNullOrWhiteSpace(json)) { return default(T); }

            var resultContent = JsonConvert.DeserializeObject<T>(json);

            return resultContent;
        }
    }
}
