using AniListClient.Models;
using System;

namespace AniListClient.Enums
{
    public static class LanguageEnumExtensions
    {
        public static string ToQueryName(this Language language) => language.ToString().ToUpper();
    }
}
