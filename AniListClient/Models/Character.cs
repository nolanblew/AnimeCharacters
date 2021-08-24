using System.Collections.Generic;

namespace AniListClient.Models
{
    public record Character(
        int Id,
        Names Name,
        Images Image,
        string Description,
        CharacterRole? Role,
        List<MediaBase> Media,
        List<VoiceActorSlim> VoiceActors);

    public record Names(
        string Romaji,
        string First,
        string Last,
        string Full,
        string Native,
        string Alternative,
        string AlternativeSpoiler);

    public enum CharacterRole
    {
        Main,
        Supporting,
        Background,
    }
}
