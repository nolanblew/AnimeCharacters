using System;

namespace Kitsu.Responses
{
    public class AnimeGetWithRelationshipsResponse
    {
        public Anime data { get; set; }
        public Included[] included { get; set; }

        public class Anime
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public Links Links { get; set; }
            public Attributes Attributes { get; set; }
            public Relationships Relationships { get; set; }
        }

        public class Links
        {
            public string Self { get; set; }
        }

        public class Attributes
        {
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public string Slug { get; set; }
            public string Synopsis { get; set; }
            public int? CoverImageTopOffset { get; set; }
            public Titles Titles { get; set; }
            public string CanonicalTitle { get; set; }
            public string[] AbbreviatedTitles { get; set; }
            public string AverageRating { get; set; }
            public Ratingfrequencies RatingFrequencies { get; set; }
            public int? UserCount { get; set; }
            public int? FavoritesCount { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public object NextRelease { get; set; }
            public int? PopularityRank { get; set; }
            public int? RatingRank { get; set; }
            public string AgeRating { get; set; }
            public string AgeRatingGuide { get; set; }
            public string Subtype { get; set; }
            public string Status { get; set; }
            public string Tba { get; set; }
            public Posterimage PosterImage { get; set; }
            public object CoverImage { get; set; }
            public int? EpisodeCount { get; set; }
            public int? EpisodeLength { get; set; }
            public int? TotalLength { get; set; }
            public string YoutubeVideoId { get; set; }
            public string ShowType { get; set; }
            public bool Nsfw { get; set; }
        }

        public class Titles
        {
            public string En { get; set; }
            public string EnJp { get; set; }
            public string JaJp { get; set; }
        }

        public class Ratingfrequencies
        {
            public string _2 { get; set; }
            public string _3 { get; set; }
            public string _4 { get; set; }
            public string _5 { get; set; }
            public string _6 { get; set; }
            public string _7 { get; set; }
            public string _8 { get; set; }
            public string _9 { get; set; }
            public string _10 { get; set; }
            public string _11 { get; set; }
            public string _12 { get; set; }
            public string _13 { get; set; }
            public string _14 { get; set; }
            public string _15 { get; set; }
            public string _16 { get; set; }
            public string _17 { get; set; }
            public string _18 { get; set; }
            public string _19 { get; set; }
            public string _20 { get; set; }
        }

        public class Posterimage
        {
            public string Tiny { get; set; }
            public string Small { get; set; }
            public string Medium { get; set; }
            public string Large { get; set; }
            public string Original { get; set; }
            public Meta Meta { get; set; }
        }

        public class Meta
        {
            public Dimensions Dimensions { get; set; }
        }

        public class Dimensions
        {
            public Tiny Tiny { get; set; }
            public Small Small { get; set; }
            public Medium Medium { get; set; }
            public Large Large { get; set; }
        }

        public class Tiny
        {
            public int? Width { get; set; }
            public int? Height { get; set; }
        }

        public class Small
        {
            public int? Width { get; set; }
            public int? Height { get; set; }
        }

        public class Medium
        {
            public int? Width { get; set; }
            public int? Height { get; set; }
        }

        public class Large
        {
            public int? Width { get; set; }
            public int? Height { get; set; }
        }

        public class Relationships
        {
            public Genres Genres { get; set; }
            public Categories Categories { get; set; }
            public Castings Castings { get; set; }
            public Installments Installments { get; set; }
            public Mappings Mappings { get; set; }
            public Reviews Reviews { get; set; }
            public Mediarelationships MediaRelationships { get; set; }
            public Characters Characters { get; set; }
            public Staff Staff { get; set; }
            public Productions Productions { get; set; }
            public Quotes Quotes { get; set; }
            public Episodes Episodes { get; set; }
            public Streaminglinks StreamingLinks { get; set; }
            public Animeproductions AnimeProductions { get; set; }
            public Animecharacters AnimeCharacters { get; set; }
            public Animestaff AnimeStaff { get; set; }
        }

        public class Genres
        {
            public Links1 Links { get; set; }
        }

        public class Links1
        {
            public string Self { get; set; }
            public string Related { get; set; }
        }

        public class Categories
        {
            public Links2 Links { get; set; }
        }

        public class Links2
        {
            public string Self { get; set; }
            public string Related { get; set; }
        }

        public class Castings
        {
            public Links3 Links { get; set; }
        }

        public class Links3
        {
            public string Self { get; set; }
            public string Related { get; set; }
        }

        public class Installments
        {
            public Links4 Links { get; set; }
        }

        public class Links4
        {
            public string Self { get; set; }
            public string Related { get; set; }
        }

        public class Mappings
        {
            public Links5 Links { get; set; }
        }

        public class Links5
        {
            public string Self { get; set; }
            public string Related { get; set; }
        }

        public class Reviews
        {
            public Links6 Links { get; set; }
        }

        public class Links6
        {
            public string Self { get; set; }
            public string Related { get; set; }
        }

        public class Mediarelationships
        {
            public Links7 Links { get; set; }
            public Datum[] Data { get; set; }
        }

        public class Links7
        {
            public string Self { get; set; }
            public string Related { get; set; }
        }

        public class Datum
        {
            public string Type { get; set; }
            public string Id { get; set; }
        }

        public class Characters
        {
            public Links8 Links { get; set; }
        }

        public class Links8
        {
            public string Self { get; set; }
            public string Related { get; set; }
        }

        public class Staff
        {
            public Links9 Links { get; set; }
        }

        public class Links9
        {
            public string Self { get; set; }
            public string Related { get; set; }
        }

        public class Productions
        {
            public Links10 Links { get; set; }
        }

        public class Links10
        {
            public string Self { get; set; }
            public string Related { get; set; }
        }

        public class Quotes
        {
            public Links11 Links { get; set; }
        }

        public class Links11
        {
            public string Self { get; set; }
            public string Related { get; set; }
        }

        public class Episodes
        {
            public Links12 Links { get; set; }
        }

        public class Links12
        {
            public string Self { get; set; }
            public string Related { get; set; }
        }

        public class Streaminglinks
        {
            public Links13 Links { get; set; }
        }

        public class Links13
        {
            public string Self { get; set; }
            public string Related { get; set; }
        }

        public class Animeproductions
        {
            public Links14 Links { get; set; }
        }

        public class Links14
        {
            public string Self { get; set; }
            public string Related { get; set; }
        }

        public class Animecharacters
        {
            public Links15 Links { get; set; }
        }

        public class Links15
        {
            public string Self { get; set; }
            public string Related { get; set; }
        }

        public class Animestaff
        {
            public Links16 Links { get; set; }
        }

        public class Links16
        {
            public string Self { get; set; }
            public string Related { get; set; }
        }

        public class Included
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public Links17 Links { get; set; }
            public Attributes Attributes { get; set; }
            public Relationships Relationships { get; set; }
        }

        public class Links17
        {
            public string Self { get; set; }
        }
    }
}