namespace AnimeCharacters.Models
{
    using AnimeCharacters.Extensions;
    using System.Collections.Generic;

    public class UserSettings
    {
        public TitleType PreferredTitleType { get; set; } = TitleType.UserPreferred;
        public Dictionary<string, bool> ExtensionEnabledStates { get; set; } = new();

        public bool IsExtensionEnabled(MediaExtensionDefinition extension)
        {
            if (extension == null)
            {
                return false;
            }

            if (extension.IsCore)
            {
                return true;
            }

            if (ExtensionEnabledStates != null &&
                ExtensionEnabledStates.TryGetValue(extension.Id, out var isEnabled))
            {
                return isEnabled;
            }

            return extension.EnabledByDefault;
        }

        public void SetExtensionEnabled(string extensionId, bool isEnabled)
        {
            ExtensionEnabledStates ??= new Dictionary<string, bool>();
            ExtensionEnabledStates[extensionId] = isEnabled;
        }
    }

    public enum TitleType
    {
        UserPreferred,
        Romaji,
        English,
        Native
    }
}
