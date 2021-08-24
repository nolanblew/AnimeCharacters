using System.Collections.Generic;

namespace AniListClient.Models
{
    public record Media(
        int Id,
        Titles Title,
        string Description,
        Images Image,
        MediaStatus Status,
        List<Character> Characters
    );

    public enum MediaStatus
    {
        NotReleasedYet,
        Releasing,
        Finished,
        Cancelled,
        Hiatus,
    }
}
