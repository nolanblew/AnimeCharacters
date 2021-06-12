using Kitsu.Requests;

namespace Kitsu.Models
{
    public class User
    {
        public User() { }

        public User(UserGetResponse.Datum apiUser)
        {
            Id = int.Parse(apiUser.id);
            Name = apiUser.attributes.name;
            Username = apiUser.attributes.slug;
            AvatarUrl = apiUser.attributes.avatar?.medium ?? apiUser.attributes.avatar?.large ?? apiUser.attributes.avatar?.original;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string AvatarUrl { get; set; }
    }
}
