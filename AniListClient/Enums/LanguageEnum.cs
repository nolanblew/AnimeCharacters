using System;

namespace AniListClient.Enums
{
    public enum LanguageEnum
    {
        JAPANESE,
        ENGLISH,
        KOREAN,
        ITALIAN,
        SPANISH,
        PORTUGESE,
        FRENCH,
        GERMAN,
        HEBREW,
        HUNGARIAN,
    }

    public static class LanguageEnumExtensions
    {
        public static string ToLongName(this LanguageEnum language) =>
            language switch
            {
                LanguageEnum.JAPANESE => "Japanese",
                LanguageEnum.ENGLISH => "English",
                LanguageEnum.KOREAN => "Korean",
                LanguageEnum.ITALIAN => "Italian",
                LanguageEnum.SPANISH => "Spanish",
                LanguageEnum.PORTUGESE => "Portugese",
                LanguageEnum.FRENCH => "French",
                LanguageEnum.GERMAN => "German",
                LanguageEnum.HEBREW => "Hebrew",
                LanguageEnum.HUNGARIAN => "Hungarian",
                _ => throw new NotImplementedException()
            };
    }
}
