namespace AnimeCharacters.Models
{
    public class UserSettings
    {
        public TitleType PreferredTitleType { get; set; } = TitleType.UserPreferred;
    }

    public enum TitleType
    {
        UserPreferred,
        Romaji,
        English,
        Native
    }
}