using AnimeCharacters.Models;
using Kitsu.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Text;
using System;

namespace AnimeCharacters.Pages
{
    public partial class Characters
    {
        [Inject]
        AniListClient.AniListClient _anilistClient { get; set; }

        public User CurrentUser { get; set; }

        public AniListClient.Models.Staff CurrentPerson { get; set; }

        public List<CharacterAnimeModel> MyCharactersList { get; set; } = new();
        public List<CharacterAnimeModel> NotMyCharactersList { get; set; } = new();

        private bool _showMobileDetails = false;

        [Parameter]
        public string Id { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (!firstRender) { return; }

            if (string.IsNullOrWhiteSpace(Id))
            {
                NavigationManager.NavigateTo("/");
                return;
            }

            CurrentUser = await DatabaseProvider.GetUserAsync();
            CurrentPerson = await _anilistClient.Staff.GetStaffById(int.Parse(Id));

            if (CurrentPerson == null)
            {
                NavigationManager.NavigateTo("/animes");
                return;
            }

            StateHasChanged();

            await _LoadCharacters();

            StateHasChanged();
        }

        protected void _OnAnimeClicked(CharacterAnimeModel model)
        {
            if (model == null) { return; }

            NavigationManager.NavigateTo($"/animes/{model.KitsuId}");
        }

        protected void _OnCharacterClicked(CharacterAnimeModel model)
        {
        }

        async Task _LoadCharacters()
        {
            // Key by AniList anime id -> list of characters this VA voiced in that anime
            var vaRoles = new Dictionary<string, List<AniListClient.Models.Character>>();

            foreach (var character in CurrentPerson.Characters.Where(role => role.Media != null))
            {
                foreach (var mediaItem in character.Media)
                {
                    if (!vaRoles.TryGetValue(mediaItem.Id.ToString(), out var list))
                    {
                        list = new List<AniListClient.Models.Character>();
                        vaRoles[mediaItem.Id.ToString()] = list;
                    }

                    // Preserve order of characters as returned from the API
                    list.Add(character);
                }
            }

            var libraryEntries = await DatabaseProvider.GetLibrariesAsync();

            MyCharactersList =
                libraryEntries.Where(libraryEntry =>
                                              !string.IsNullOrWhiteSpace(libraryEntry.Anime.AnilistId) &&
                                              vaRoles.ContainsKey(libraryEntry.Anime.AnilistId))
                              .SelectMany(libraryEntry =>
                              {
                                  var characters = vaRoles[libraryEntry.Anime.AnilistId];
                                  return characters.Select(character => new CharacterAnimeModel
                                  {
                                      KitsuId = libraryEntry.Anime.KitsuId,
                                      AnimeImageUrl = libraryEntry.Anime.PosterImageUrl,
                                      LastProgressedAt = libraryEntry.ProgressedAt,
                                      VoiceActingRole = character,
                                  });
                              })
                              .OrderByDescending(item => item.LastProgressedAt ?? System.DateTimeOffset.MinValue)
                              .ToList();

        }

        private void ToggleMobileDetails()
        {
            _showMobileDetails = !_showMobileDetails;
        }

        private bool HasVADetails()
        {
            if (CurrentPerson == null) return false;

            return CurrentPerson.Age.HasValue ||
                   (CurrentPerson.DateOfBirth != null && (CurrentPerson.DateOfBirth.Year.HasValue || CurrentPerson.DateOfBirth.Month.HasValue)) ||
                   !string.IsNullOrWhiteSpace(CurrentPerson.BloodType) ||
                   !string.IsNullOrWhiteSpace(ParseBiographyToHtml()) ||
                   ExtractSocialMediaLinks().Any();
        }

        private string GetFormattedBirthDate()
        {
            if (CurrentPerson?.DateOfBirth == null) return "";

            var parts = new List<string>();

            if (CurrentPerson.DateOfBirth.Month.HasValue)
            {
                var monthNames = new[] { "", "January", "February", "March", "April", "May", "June",
                                       "July", "August", "September", "October", "November", "December" };
                parts.Add(monthNames[CurrentPerson.DateOfBirth.Month.Value]);
            }

            if (CurrentPerson.DateOfBirth.Day.HasValue && CurrentPerson.DateOfBirth.Month.HasValue)
            {
                parts.Add(CurrentPerson.DateOfBirth.Day.Value.ToString());
            }

            if (CurrentPerson.DateOfBirth.Year.HasValue)
            {
                parts.Add(CurrentPerson.DateOfBirth.Year.Value.ToString());
            }

            return string.Join(" ", parts);
        }

        private List<SocialMediaLink> ExtractSocialMediaLinks()
        {
            var links = new List<SocialMediaLink>();

            if (string.IsNullOrWhiteSpace(CurrentPerson?.Description))
                return links;

            // Add AniList profile link if available
            if (!string.IsNullOrWhiteSpace(CurrentPerson.SiteUrl))
            {
                links.Add(new SocialMediaLink
                {
                    Platform = "AniList",
                    Url = CurrentPerson.SiteUrl,
                    DisplayText = "anilist.co",
                    IconPath = "/images/anilist-icon.svg"
                });
            }

            // Extract markdown links from biography
            var markdownLinkPattern = @"\[([^\]]+)\]\(([^)]+)\)";
            var matches = Regex.Matches(CurrentPerson.Description, markdownLinkPattern);

            foreach (Match match in matches)
            {
                var text = match.Groups[1].Value;
                var url = match.Groups[2].Value;

                var socialLink = CreateSocialMediaLink(text, url);
                if (socialLink != null)
                {
                    links.Add(socialLink);
                }
            }

            return links;
        }

        private SocialMediaLink CreateSocialMediaLink(string text, string url)
        {
            var lowerUrl = url.ToLower();
            var lowerText = text.ToLower();

            // Twitter/X
            if (lowerUrl.Contains("twitter.com") || lowerUrl.Contains("x.com") || lowerText.Contains("twitter"))
            {
                return new SocialMediaLink
                {
                    Platform = "X",
                    Url = url,
                    DisplayText = "x.com",
                    IconPath = "/images/x-icon.svg"
                };
            }

            // Instagram
            if (lowerUrl.Contains("instagram.com") || lowerText.Contains("instagram"))
            {
                return new SocialMediaLink
                {
                    Platform = "Instagram",
                    Url = url,
                    DisplayText = "instagram.com",
                    IconPath = "/images/instagram-icon.svg"
                };
            }

            // YouTube
            if (lowerUrl.Contains("youtube.com") || lowerUrl.Contains("youtu.be") || lowerText.Contains("youtube"))
            {
                return new SocialMediaLink
                {
                    Platform = "YouTube",
                    Url = url,
                    DisplayText = "youtube.com",
                    IconPath = "/images/youtube-icon.svg"
                };
            }

            // TikTok
            if (lowerUrl.Contains("tiktok.com") || lowerText.Contains("tiktok"))
            {
                return new SocialMediaLink
                {
                    Platform = "TikTok",
                    Url = url,
                    DisplayText = "tiktok.com",
                    IconPath = "/images/tiktok-icon.svg"
                };
            }

            // Facebook
            if (lowerUrl.Contains("facebook.com") || lowerText.Contains("facebook"))
            {
                return new SocialMediaLink
                {
                    Platform = "Facebook",
                    Url = url,
                    DisplayText = "facebook.com",
                    IconPath = "/images/facebook-icon.svg"
                };
            }

            // Profile/Personal Website (catch-all for other profile links)
            if (lowerText.Contains("profile") || lowerText.Contains("website") || lowerText.Contains("official"))
            {
                var domain = ExtractDomain(url);
                return new SocialMediaLink
                {
                    Platform = "Website",
                    Url = url,
                    DisplayText = domain,
                    IconPath = "/images/website-icon.svg"
                };
            }

            return null; // Don't include unrecognized links in social media section
        }

        private string ExtractDomain(string url)
        {
            try
            {
                var uri = new Uri(url);
                return uri.Host.StartsWith("www.") ? uri.Host.Substring(4) : uri.Host;
            }
            catch
            {
                return url;
            }
        }

        private string ParseBiographyToHtml()
        {
            if (string.IsNullOrWhiteSpace(CurrentPerson?.Description))
                return string.Empty;

            var content = CurrentPerson.Description;

            // Convert markdown links to HTML links, but exclude social media links that we've moved to the Links section
            var markdownLinkPattern = @"\[([^\]]+)\]\(([^)]+)\)";
            content = Regex.Replace(content, markdownLinkPattern, match =>
            {
                var text = match.Groups[1].Value;
                var url = match.Groups[2].Value;

                // Skip social media links as they're now in the Links section
                if (IsSocialMediaLink(text, url))
                {
                    return ""; // Remove from biography
                }

                // Convert other links to HTML
                return $"<a href=\"{url}\" target=\"_blank\" rel=\"noopener noreferrer\">{text}</a>";
            });

            // Clean up extra spaces and separators left by removed links
            content = Regex.Replace(content, @"\s*\|\s*$", ""); // Remove trailing |
            content = Regex.Replace(content, @"^\s*\|\s*", ""); // Remove leading |
            content = Regex.Replace(content, @"\s*\|\s*\|\s*", " | "); // Clean up double |
            content = Regex.Replace(content, @"\s+", " "); // Clean up multiple spaces
            content = content.Trim();

            // Convert line breaks to HTML
            content = content.Replace("\n", "<br>");

            return content;
        }

        private bool IsSocialMediaLink(string text, string url)
        {
            var lowerUrl = url.ToLower();
            var lowerText = text.ToLower();

            return lowerUrl.Contains("twitter.com") || lowerUrl.Contains("x.com") ||
                   lowerUrl.Contains("instagram.com") || lowerUrl.Contains("youtube.com") ||
                   lowerUrl.Contains("youtu.be") || lowerUrl.Contains("tiktok.com") ||
                   lowerUrl.Contains("facebook.com") || lowerText.Contains("twitter") ||
                   lowerText.Contains("instagram") || lowerText.Contains("youtube") ||
                   lowerText.Contains("tiktok") || lowerText.Contains("facebook") ||
                   lowerText.Contains("profile");
        }

        public class SocialMediaLink
        {
            public string Platform { get; set; }
            public string Url { get; set; }
            public string DisplayText { get; set; }
            public string IconPath { get; set; }
        }
    }
}
