using System.Collections.Generic;

namespace AniListClient.Models
{
    public record MediaBase(
        int Id,
        Titles Title);

    public record Media(
        int Id,
        Titles Title,
        string Description,
        Images Image,
        MediaStatus Status,
        List<Character> Characters
    ) : MediaBase(Id, Title);

    public enum MediaStatus
    {
        NotReleasedYet,
        Releasing,
        Finished,
        Cancelled,
        Hiatus,
    }
}
