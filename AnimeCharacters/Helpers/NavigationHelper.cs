using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Specialized;
using System.Web;

namespace AnimeCharacters.Helpers
{
    // https://jasonwatmore.com/post/2020/08/09/blazor-webassembly-get-query-string-parameters-with-navigation-manager
    public static class NavigationHelper
    {
        /// <summary>
        /// get entire querystring name/value collection
        /// </summary>
        /// <param name="navigationManager">The navigation manager for the context</param>
        /// <returns>A <see cref="NameValueCollection"/> of all the keys and values</returns>
        public static NameValueCollection QueryString(this NavigationManager navigationManager)
            => HttpUtility.ParseQueryString(new Uri(navigationManager.Uri).Query);

        /// <summary>
        /// get single querystring value with specified key
        /// </summary>
        /// <param name="navigationManager">The navigation manager for the context</param>
        /// <param name="key">The key of the query</param>
        /// <returns>The value of the query based on the <paramref name="key"/></returns>
        public static string QueryString(this NavigationManager navigationManager, string key)
        {
            return navigationManager.QueryString()[key];
        }
    }
}
