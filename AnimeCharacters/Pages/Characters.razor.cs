using AnimeCharacters.Extensions;
using AnimeCharacters.Models;
using Kitsu.Models;
using Markdig;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AnimeCharacters.Pages
{
    public partial class Characters
    {
        [Inject]
        AniListClient.AniListClient _anilistClient { get; set; }

        [Inject]
        IVoiceActorCreditService _voiceActorCreditService { get; set; }

        public User CurrentUser { get; set; }

        public AniListClient.Models.Staff CurrentPerson { get; set; }

        public List<VoiceActorCreditSection> CreditSections { get; set; } = new();

        public bool IsLoadingCharacters { get; set; }

        public string CharacterLoadError { get; set; }

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
            IsLoadingCharacters = true;
            CharacterLoadError = null;

            try
            {
                CurrentPerson = await _anilistClient.Staff.GetStaffById(int.Parse(Id));
            }
            catch (Exception ex)
            {
                CharacterLoadError = $"Unable to load characters from AniList. {ex.Message}";
                IsLoadingCharacters = false;
                StateHasChanged();
                return;
            }

            if (CurrentPerson == null)
            {
                NavigationManager.NavigateTo("/animes");
                return;
            }

            StateHasChanged();

            try
            {
                await _LoadCharacters();
            }
            catch (Exception ex)
            {
                CharacterLoadError = $"Unable to load characters. {ex.Message}";
            }
            finally
            {
                IsLoadingCharacters = false;
            }

            StateHasChanged();
        }

        protected void _OnMediaClicked(VoiceActorCreditModel model)
        {
            if (model == null) { return; }

            if (!string.IsNullOrWhiteSpace(model.MediaRoute))
            {
                NavigationManager.NavigateTo(model.MediaRoute);
            }
        }

        protected void _OnCharacterClicked(VoiceActorCreditModel model)
        {
            if (model == null) { return; }

            if (!string.IsNullOrWhiteSpace(model.CharacterRoute))
            {
                NavigationManager.NavigateTo(model.CharacterRoute);
            }
        }

        async Task _LoadCharacters()
        {
            CreditSections = (await _voiceActorCreditService.GetCreditSectionsAsync(CurrentPerson)).ToList();
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

            return null;
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
            var markdownLinkPattern = @"\[([^\]]+)\]\(([^)]+)\)";
            content = Regex.Replace(content, markdownLinkPattern, match =>
            {
                var text = match.Groups[1].Value;
                var url = match.Groups[2].Value;

                return IsSocialMediaLink(text, url) ? string.Empty : match.Value;
            });

            var pipeline = new MarkdownPipelineBuilder()
                                .UseAdvancedExtensions()
                                .Build();
            var html = Markdown.ToHtml(content, pipeline);

            html = Regex.Replace(html, "<a href=\"([^\"]+)\"", "<a href=\"$1\" target=\"_blank\" rel=\"noopener noreferrer\"");

            return html.Trim();
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
