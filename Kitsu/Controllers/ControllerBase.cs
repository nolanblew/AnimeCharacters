using System;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace Kitsu.Controllers
{
    public abstract class ControllerBase
    {
        public ControllerBase(string baseUrl, string controllerRoute)
        {
            _baseUrl = new Uri(new Uri(baseUrl), controllerRoute);
        }

        protected readonly string _controllerRoute;

        protected readonly Uri _baseUrl;

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
    }
}
