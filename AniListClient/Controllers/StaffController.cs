using AniListClient.Converters;
using AniListClient.Models;
using AniListClient.Queries;
using AniListClient.Responses.AniListClient.Responses;
using GraphQL.Client.Http;
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
    }
}
