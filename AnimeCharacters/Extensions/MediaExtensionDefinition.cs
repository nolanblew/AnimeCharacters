namespace AnimeCharacters.Extensions
{
    /// <summary>
    /// Describes a user-visible media extension.
    /// </summary>
    public class MediaExtensionDefinition
    {
        public string Id { get; init; }
        public string Name { get; init; }
        public string CategoryName { get; init; }
        public string Description { get; init; }
        public bool IsCore { get; init; }
        public bool EnabledByDefault { get; init; }
    }
}
