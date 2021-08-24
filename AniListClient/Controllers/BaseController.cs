using AniListClient.Queries;
using GraphQL;
using GraphQL.Client.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AniListClient.Controllers
{
    public abstract class BaseController
    {
        internal readonly GraphQLHttpClient _graphQLHttpClient;

        internal BaseController(GraphQLHttpClient graphQLHttpClient)
        {
            _graphQLHttpClient = graphQLHttpClient;
        }

        internal async Task<List<TResult>> GetPaginatedList<TBase,TResult>(string query, string operationName, IHasPage variables, Func<TBase, IList<TResult>> selector, Func<TBase, bool> hasMorePagesFunc)
        {
            var returnList = new List<TResult>();
            var page = 1;

            var request = new GraphQLRequest(query);
            request.OperationName = operationName;

            while (true)
            {
                variables.Page = page++;
                request.Variables = variables;

                var result = await _graphQLHttpClient.SendQueryAsync<TBase>(request);
                returnList.AddRange(selector(result.Data));

                if (!hasMorePagesFunc(result.Data))
                {
                    return returnList;
                }
            }
        }
    }
}
