using System.Collections.Generic;

namespace AniListClient.Models
{
    public record Character(
        int Id,
        CharacterName Name,
        Images Image,
        CharacterRole? Role,
        List<VoiceActorSlim> VoiceActors);

    public record CharacterName(
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
