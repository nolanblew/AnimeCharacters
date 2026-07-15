using AniListClient.Converters;
using AniListClient.Models;
using AniListClient.Queries;
using AniListClient.Responses;
using AniListClient.Responses.AniListClient.Responses;
using GraphQL;
using GraphQL.Client.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AniListClient.Controllers
{
    public class StaffController : BaseController
    {
        public StaffController(GraphQLHttpClient graphQLHttpClient)
            : base(graphQLHttpClient) { }

        public Task<Staff> GetStaffById(int id) =>
            GetPaginatedList<StaffQueryResponse, Staff, Character>(
                StaffQueries.GET_STAFF_BY_ID_QUERY,
                StaffQueries.GET_STAFF_BY_ID_NAME,
                new StaffQueryVariable(id),
                q => q.Characters,
                q => q.Staff.ToStaff(),
                q => q.Staff.Characters?.PageInfo?.HasNextPage ?? false);

        public Task<Staff> GetStaffByName(string name) =>
            GetPaginatedList<StaffQueryResponse, Staff, Character>(
                StaffQueries.GET_STAFF_BY_NAME_QUERY,
                StaffQueries.GET_STAFF_BY_NAME_NAME,
                new StaffNameQueryVariable(name),
                q => q.Characters,
                q => q.Staff.ToStaff(),
                q => q.Staff.Characters?.PageInfo?.HasNextPage ?? false);

        public async Task<IReadOnlyList<Staff>> SearchStaffByName(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return new List<Staff>();
            }

            var request = new GraphQLRequest(StaffQueries.SEARCH_STAFF_BY_NAME_QUERY)
            {
                OperationName = StaffQueries.SEARCH_STAFF_BY_NAME_NAME,
                Variables = new StaffSearchVariable(search)
            };

            GraphQLResponse<SearchStaffResponse> result;
            try
            {
                result = await _graphQLHttpClient.SendQueryAsync<SearchStaffResponse>(request);
            }
            catch (GraphQLHttpRequestException ex)
            {
                throw new InvalidOperationException(GetGraphQLHttpErrorMessage(ex), ex);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException("Unable to reach AniList. The API may be unavailable right now.", ex);
            }

            if (result.Errors?.Length > 0)
            {
                throw new InvalidOperationException(result.Errors[0].Message);
            }

            return result.Data?.Page?.Staff?
                .Where(staff => staff.LanguageV2 == Language.Japanese)
                .Select(staff => staff.ToStaff())
                .ToList() ?? new List<Staff>();
        }
    }
}
