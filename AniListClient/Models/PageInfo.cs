namespace AniListClient.Models
{
    public interface IHasPageInfo
    {
        PageInfo PageInfo { get; }
    }

    public record PageInfo(
        int Total,
        int PerPage,
        int CurrentPage,
        int LastPage,
        int HasNextPage);
}
