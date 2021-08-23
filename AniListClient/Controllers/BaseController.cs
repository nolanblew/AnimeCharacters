using AniListClient.Models;
using GraphQL;
using GraphQL.Client.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AniListClient.Controllers
{
    public abstract class BaseController
    {
        protected readonly GraphQLHttpClient _graphQLHttpClient;

        public BaseController(GraphQLHttpClient graphQLHttpClient)
        {
            _graphQLHttpClient = graphQLHttpClient;
        }

        public List<TResult> GetPaginatedList<TBase,TResult>(string query, Func<TBase, TResult> Selector) where TResult : IHasPageInfo
        {
            var returnList = new List<TResult>();
            var page = 1;

            var request = new GraphQLRequest(query);
            request.Variables = 

            while (true)
            {
                _graphQLHttpClient.
            }
        }

        
        
        _List<TResult> GetPaginatedList<TBase,TResult>(string query, Func<TBase, TResult> Selector, int page) where TResult : IHasPageInfo
        {

        }
    }
}
