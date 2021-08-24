using AniListClient.Queries;
using GraphQL;
using GraphQL.Client.Http;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
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

        /// <summary>
        /// Gets a paginated list of items that are nested in a request
        /// </summary>
        /// <typeparam name="TBase">The base type to directly convert the request to</typeparam>
        /// <typeparam name="TResult">The base result type you'd like to have at the end</typeparam>
        /// <typeparam name="TListType">The type that will be paginated in a list</typeparam>
        /// <param name="query">The GraphQL Query</param>
        /// <param name="operationName">The operation of the GraphQL. Note: As of now all queries must be in "operation" form</param>
        /// <param name="variables">The variables to pass in. This must implement <see cref="IHasPage"/> to allow this method to modify and pass through the paging</param>
        /// <param name="selectorExpression">The expression that points to the PROPERTY (with public get and public set) that holds the list of <typeparamref name="TListType"/> that we want to paginate on</param>
        /// <param name="conversionSelector">How to convert from <typeparamref name="TBase"/> to <typeparamref name="TResult"/></param>
        /// <param name="hasMorePagesFunc">The property or func that tells us when we have more items to page through</param>
        /// <returns></returns>
        internal async Task<TResult> GetPaginatedList<TBase,TResult,TListType>(
            string query,
            string operationName,
            IHasPage variables,
            Expression<Func<TResult, List<TListType>>> selectorExpression,
            Func<TBase, TResult> conversionSelector,
            Func<TBase, bool> hasMorePagesFunc)
        {
            // Assign the getter/setter
            var selector = selectorExpression.Compile();
            
            var prop = (PropertyInfo)((MemberExpression)selectorExpression.Body).Member;

            void assigner(TResult model, List<TListType> list) => prop.SetValue(model, list);

            var returnList = new List<TListType>();
            var page = 1;

            var request = new GraphQLRequest(query);
            request.OperationName = operationName;

            while (true)
            {
                variables.Page = page++;
                request.Variables = variables;

                var result = await _graphQLHttpClient.SendQueryAsync<TBase>(request);
                var model = conversionSelector(result.Data);
                returnList.AddRange(selector(model));

                if (!hasMorePagesFunc(result.Data))
                {
                    assigner(model, returnList);

                    return model;
                }
            }
        }
    }
}
