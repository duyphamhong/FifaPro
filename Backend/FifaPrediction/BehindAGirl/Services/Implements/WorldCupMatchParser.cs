using BehindAGirl.Common.Constants;
using BehindAGirl.Common.Extensions;
using HtmlAgilityPack;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace BehindAGirl.Services.Implements
{
    internal static class WorldCupMatchParser
    {
        public const string TournamentUrl = "https://en.wikipedia.org/wiki/2026_FIFA_World_Cup";

        public static string GetScoreText(HtmlNode matchNode)
        {
            return NormalizeWikipediaText(matchNode
                ?.GetChildNode("th", "class", "fscore", CompareModeEnum.Equal)
                ?.InnerText);
        }

        public static bool TryGetScore(HtmlNode matchNode, out int homeScore, out int awayScore)
        {
            homeScore = 0;
            awayScore = 0;

            var scoreText = GetScoreText(matchNode);
            var match = Regex.Match(scoreText, @"^\s*(\d+)\s*-\s*(\d+)");
            if (!match.Success)
            {
                return false;
            }

            homeScore = int.Parse(match.Groups[1].Value);
            awayScore = int.Parse(match.Groups[2].Value);
            return true;
        }

        public static string GetHomeTeam(HtmlNode matchNode)
        {
            return GetTeamName(matchNode, "fhome");
        }

        public static string GetAwayTeam(HtmlNode matchNode)
        {
            return GetTeamName(matchNode, "faway");
        }

        public static string NormalizeTeamName(string teamName)
        {
            var normalized = NormalizeWikipediaText(teamName);
            return normalized == "Czechia" ? "Czech Republic" : normalized;
        }

        public static string GetStadium(HtmlNode matchNode)
        {
            return NormalizeWikipediaText(matchNode
                ?.GetChildNode("div", "itemprop", "location", CompareModeEnum.Equal)
                ?.InnerText);
        }

        public static string GetMatchDetailUrl(HtmlNode matchNode)
        {
            var reportUrl = FindReportUrl(matchNode);
            if (string.IsNullOrWhiteSpace(reportUrl))
            {
                return TournamentUrl;
            }

            return reportUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? reportUrl
                : "https://en.wikipedia.org" + reportUrl;
        }

        public static string NormalizeWikipediaText(string text)
        {
            var normalized = HtmlEntity.DeEntitize(text ?? string.Empty)
                .Replace('\u00A0', ' ')
                .Replace("&#160;", " ")
                .Replace("&#160", " ")
                .Replace("−", "-")
                .Replace("–", "-");

            return Regex.Replace(normalized, @"\s+", " ").Trim();
        }

        private static string GetTeamName(HtmlNode matchNode, string teamClass)
        {
            var teamNode = matchNode?.GetChildNode("th", "class", teamClass, CompareModeEnum.Equal);
            var teamLinkText = teamNode
                ?.Descendants("a")
                .Select(node => NormalizeWikipediaText(node.InnerText))
                .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));

            return NormalizeTeamName(string.IsNullOrWhiteSpace(teamLinkText)
                ? teamNode?.InnerText
                : teamLinkText);
        }

        private static string FindReportUrl(HtmlNode matchNode)
        {
            var scoreNode = matchNode?.GetChildNode("th", "class", "fscore", CompareModeEnum.Equal);
            var goalsNode = matchNode?.GetChildNode("tr", "class", "fgoals", CompareModeEnum.Equal);

            return new[] { scoreNode, goalsNode }
                .Where(node => node != null)
                .SelectMany(node => node.Descendants("a"))
                .Where(IsReportLink)
                .Select(node => node.GetAttributeValue("href", "").Trim())
                .FirstOrDefault(IsReportUrl);
        }

        private static bool IsReportLink(HtmlNode linkNode)
        {
            var linkClass = linkNode.GetAttributeValue("class", "");
            var linkText = NormalizeWikipediaText(linkNode.InnerText);

            return linkClass.Contains("external text", StringComparison.OrdinalIgnoreCase)
                || linkText.StartsWith("Report", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsReportUrl(string href)
        {
            if (string.IsNullOrWhiteSpace(href))
            {
                return false;
            }

            if (href.StartsWith("#", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return href.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                || href.StartsWith("/wiki/", StringComparison.OrdinalIgnoreCase);
        }
    }
}
