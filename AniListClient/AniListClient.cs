using AniListClient.Controllers;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;

namespace AniListClient
{
    public class AniListClient
    {
        public AniListClient()
        {
            Characters = new(_graphQLClient);
            Staff = new(_graphQLClient);
        }

        GraphQLHttpClient _graphQLClient = new("https://graphql.anilist.co", new NewtonsoftJsonSerializer());

        public CharactersController Characters { get; }
        public StaffController Staff { get; }
    }
}
