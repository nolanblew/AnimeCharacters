using System;

namespace Kitsu.Requests
{

    public class UserGetResponse
    {
        public Datum[] data { get; set; }
        public Meta meta { get; set; }
        public Links links { get; set; }

        public class Meta
        {
            public int count { get; set; }
        }

        public class Links
        {
            public string first { get; set; }
            public string next { get; set; }
            public string last { get; set; }
        }

        public class Datum
        {
            public string id { get; set; }
            public string type { get; set; }
            public Links1 links { get; set; }
            public Attributes attributes { get; set; }
            public Relationships relationships { get; set; }
        }

        public class Links1
        {
            public string self { get; set; }
        }

        public class Attributes
        {
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
            public string name { get; set; }
            public string[] pastNames { get; set; }
            public string slug { get; set; }
            public string about { get; set; }
            public string location { get; set; }
            public object waifuOrHusbando { get; set; }
            public int followersCount { get; set; }
            public int followingCount { get; set; }
            public int lifeSpentOnAnime { get; set; }
            public object birthday { get; set; }
            public string gender { get; set; }
            public int commentsCount { get; set; }
            public int favoritesCount { get; set; }
            public int likesGivenCount { get; set; }
            public int reviewsCount { get; set; }
            public int likesReceivedCount { get; set; }
            public int postsCount { get; set; }
            public int ratingsCount { get; set; }
            public int mediaReactionsCount { get; set; }
            public object proExpiresAt { get; set; }
            public object title { get; set; }
            public bool profileCompleted { get; set; }
            public bool feedCompleted { get; set; }
            public object website { get; set; }
            public Avatar avatar { get; set; }
            public Coverimage coverImage { get; set; }
            public string status { get; set; }
            public bool subscribedToNewsletter { get; set; }
        }

        public class Avatar
        {
            public string tiny { get; set; }
            public string small { get; set; }
            public string medium { get; set; }
            public string large { get; set; }
            public string original { get; set; }
            public Meta1 meta { get; set; }
        }

        public class Meta1
        {
            public Dimensions dimensions { get; set; }
        }

        public class Dimensions
        {
            public Tiny tiny { get; set; }
            public Small small { get; set; }
            public Medium medium { get; set; }
            public Large large { get; set; }
        }

        public class Tiny
        {
            public int? width { get; set; }
            public int? height { get; set; }
        }

        public class Small
        {
            public int? width { get; set; }
            public int? height { get; set; }
        }

        public class Medium
        {
            public int? width { get; set; }
            public int? height { get; set; }
        }

        public class Large
        {
            public int? width { get; set; }
            public int? height { get; set; }
        }

        public class Coverimage
        {
            public string tiny { get; set; }
            public string small { get; set; }
            public string large { get; set; }
            public string original { get; set; }
            public Meta2 meta { get; set; }
        }

        public class Meta2
        {
            public Dimensions1 dimensions { get; set; }
        }

        public class Dimensions1
        {
            public Tiny1 tiny { get; set; }
            public Small1 small { get; set; }
            public Large1 large { get; set; }
        }

        public class Tiny1
        {
            public int? width { get; set; }
            public int? height { get; set; }
        }

        public class Small1
        {
            public int? width { get; set; }
            public int? height { get; set; }
        }

        public class Large1
        {
            public int? width { get; set; }
            public int? height { get; set; }
        }

        public class Relationships
        {
            public Waifu waifu { get; set; }
            public Pinnedpost pinnedPost { get; set; }
            public Followers followers { get; set; }
            public Following following { get; set; }
            public Blocks blocks { get; set; }
            public Linkedaccounts linkedAccounts { get; set; }
            public Profilelinks profileLinks { get; set; }
            public Userroles userRoles { get; set; }
            public Libraryentries libraryEntries { get; set; }
            public Favorites favorites { get; set; }
            public Reviews reviews { get; set; }
            public Stats stats { get; set; }
            public Notificationsettings notificationSettings { get; set; }
            public Onesignalplayers oneSignalPlayers { get; set; }
            public Categoryfavorites categoryFavorites { get; set; }
            public Quotes quotes { get; set; }
        }

        public class Waifu
        {
            public Links2 links { get; set; }
        }

        public class Links2
        {
            public string self { get; set; }
            public string related { get; set; }
        }

        public class Pinnedpost
        {
            public Links3 links { get; set; }
        }

        public class Links3
        {
            public string self { get; set; }
            public string related { get; set; }
        }

        public class Followers
        {
            public Links4 links { get; set; }
        }

        public class Links4
        {
            public string self { get; set; }
            public string related { get; set; }
        }

        public class Following
        {
            public Links5 links { get; set; }
        }

        public class Links5
        {
            public string self { get; set; }
            public string related { get; set; }
        }

        public class Blocks
        {
            public Links6 links { get; set; }
        }

        public class Links6
        {
            public string self { get; set; }
            public string related { get; set; }
        }

        public class Linkedaccounts
        {
            public Links7 links { get; set; }
        }

        public class Links7
        {
            public string self { get; set; }
            public string related { get; set; }
        }

        public class Profilelinks
        {
            public Links8 links { get; set; }
        }

        public class Links8
        {
            public string self { get; set; }
            public string related { get; set; }
        }

        public class Userroles
        {
            public Links9 links { get; set; }
        }

        public class Links9
        {
            public string self { get; set; }
            public string related { get; set; }
        }

        public class Libraryentries
        {
            public Links10 links { get; set; }
        }

        public class Links10
        {
            public string self { get; set; }
            public string related { get; set; }
        }

        public class Favorites
        {
            public Links11 links { get; set; }
        }

        public class Links11
        {
            public string self { get; set; }
            public string related { get; set; }
        }

        public class Reviews
        {
            public Links12 links { get; set; }
        }

        public class Links12
        {
            public string self { get; set; }
            public string related { get; set; }
        }

        public class Stats
        {
            public Links13 links { get; set; }
        }

        public class Links13
        {
            public string self { get; set; }
            public string related { get; set; }
        }

        public class Notificationsettings
        {
            public Links14 links { get; set; }
        }

        public class Links14
        {
            public string self { get; set; }
            public string related { get; set; }
        }

        public class Onesignalplayers
        {
            public Links15 links { get; set; }
        }

        public class Links15
        {
            public string self { get; set; }
            public string related { get; set; }
        }

        public class Categoryfavorites
        {
            public Links16 links { get; set; }
        }

        public class Links16
        {
            public string self { get; set; }
            public string related { get; set; }
        }

        public class Quotes
        {
            public Links17 links { get; set; }
        }

        public class Links17
        {
            public string self { get; set; }
            public string related { get; set; }
        }
    }
}
