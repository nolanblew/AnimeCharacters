using System.Collections.Generic;

namespace AniListClient.Models
{
    public record MediaBase(
        int Id,
        Titles Title,
        string ProviderName = "anilist");

    public record Media(
        int Id,
        Titles Title,
        string Description,
        Images Image,
        MediaStatus Status,
        List<Character> Characters,
        string ProviderName = "anilist"
    ) : MediaBase(Id, Title, ProviderName);

    public enum MediaStatus
    {
        NotReleasedYet,
        Releasing,
        Finished,
        Cancelled,
        Hiatus,
    }
}
