using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using System;

namespace AniListClient
{
    public class AniListClient
    {
        GraphQLHttpClient _graphQLClient = new("", new NewtonsoftJsonSerializer());


    }
}
