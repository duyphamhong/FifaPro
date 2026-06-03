using BehindAGirl.Common.Constants;
using BehindAGirl.Common.Extensions;
using BehindAGirl.Data;
using BehindAGirl.Repositoties.Interfaces;
using BehindAGirl.Services.Interfaces;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BehindAGirl.Services.Implements
{
    public class CrawlerService : ICrawlerService
    {
        private readonly IDataInformationRepository _dataInformationRepository;
        public CrawlerService(IDataInformationRepository dataInformationRepository)
        {
            _dataInformationRepository = dataInformationRepository;
        }

        public async Task GetStandings()
        {
            try
            {
                //Change the list depending on the group stage of the tournament
                var groupsName = new List<string>() { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L" };
                var url = "https://en.wikipedia.org/wiki/2026_FIFA_World_Cup";
                var web = new HtmlWeb();
                var htmlDocument = await web.LoadFromWebAsync(url);

                var groupElements = htmlDocument.DocumentNode.Descendants("table").Where(node => node.GetAttributeValue("class", "").Equals("wikitable") && node.GetAttributeValue("style", "").Equals("text-align:center;")).ToList();

                var groups = new List<Group>();

                var index = 0;

                foreach (var element in groupElements)
                {
                    var teamTrs = element.Descendants("tbody").FirstOrDefault().Descendants("tr");
                    var teamDivs = element.Descendants("tbody").FirstOrDefault().Descendants("tr").ToList();

                    if (teamTrs.Count() != 5)
                    {
                        continue;
                    }

                    var group = new Group
                    {
                        Id = Guid.NewGuid(),
                        Name = groupsName[index]
                    };
                    index++;

                    group.Teams = new List<Team>();


                    teamDivs.RemoveAt(0);
                    foreach (var teamDiv in teamDivs)
                    {
                        if (teamDiv.Descendants("td").ToArray().Count() <= 10)
                        {
                            var team = new Team
                            {
                                Id = Guid.NewGuid(),
                                GroupId = group.Id,

                                Name = teamDiv.Descendants("th").FirstOrDefault()
                            .Descendants("a").FirstOrDefault().InnerText.TrimInnerText(),

                                Played = teamDiv.Descendants("td").ToArray()[1].InnerText.TrimInnerText().ToInt(),

                                Won = teamDiv.Descendants("td").ToArray()[2].InnerText.TrimInnerText().ToInt(),
                                Drawn = teamDiv.Descendants("td").ToArray()[3].InnerText.TrimInnerText().ToInt(),
                                Against = teamDiv.Descendants("td").ToArray()[4].InnerText.TrimInnerText().ToInt(),
                                For = teamDiv.Descendants("td").ToArray()[5].InnerText.TrimInnerText().ToInt(),
                                Goals = teamDiv.Descendants("td").ToArray()[5].InnerText.TrimInnerText().ToInt(),
                                Lost = teamDiv.Descendants("td").ToArray()[6].InnerText.TrimInnerText().ToInt(),
                                Points = teamDiv.Descendants("td").ToArray()[7].InnerText.TrimInnerText().ToInt(),
                                LogoUrl = teamDiv.Descendants("img").FirstOrDefault().GetAttributeValue("src", "").TrimInnerText()
                            };
                            team.Code = team.Name;
                            group.Teams.Add(team);
                        }
                    }

                    groups.Add(group);
                }

                await _dataInformationRepository.UpdateStandings(groups);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task GetMatches()
        {
            try
            {
                var url = "https://en.wikipedia.org/wiki/2026_FIFA_World_Cup";
                var web = new HtmlWeb();
                var htmlDocument = await web.LoadFromWebAsync(url);

                var divs = htmlDocument.DocumentNode.GetChildNodeList("div", "class", "footballbox", Common.Constants.CompareModeEnum.Equal);

                var macthes = new List<Match>();

                foreach (var div in divs)
                {
                    var match = new Match();

                    match.Id = Guid.NewGuid();
                    match.Name = div.GetChildNode("th", "class", "fscore", Common.Constants.CompareModeEnum.Equal)
                                .ChildNodes.FirstOrDefault().InnerText.TrimInnerText();
                    match.DetailUrl = GetMatchDetailUrl(div);
                    match.KickOfDate = GetUtcKickOffDate(div);
                    //ImageUrl = div.GetChildNode("meta", "itemprop", "image", Common.Constants.CompareModeEnum.Equal).GetAttributeValue("content","").TrimInnerText(),
                    // Description = div.GetChildNode("meta", "itemprop", "description", Common.Constants.CompareModeEnum.Equal).GetAttributeValue("content","").TrimInnerText(),
                    match.Stadium = div.GetChildNode("div", "itemprop", "location", Common.Constants.CompareModeEnum.Equal).InnerText.TrimInnerText();
                    match.Team1 = div.GetChildNode("th", "class", "fhome", Common.Constants.CompareModeEnum.Equal).InnerText.TrimInnerText();
                    match.Team2 = div.GetChildNode("th", "class", "faway", Common.Constants.CompareModeEnum.Equal).InnerText.TrimInnerText();

                    /*** FOR WORLDCUP ***/

                    if (match.Name.IndexOf('–') > 0)
                    {
                        var scored = match.Name.Split("–");
                        match.Team1Scored = scored[0].ToIntNullable();
                        match.Team2Scored = scored[1].ToIntNullable();
                        match.Winner = (match.Team1Scored == null || match.Team1Scored == match.Team2Scored) ? null : match.Team1Scored > match.Team2Scored ? match.Team1 : match.Team2;
                        match.Draw = match.Team1Scored != null && match.Team1Scored == match.Team2Scored ? true : null;
                    }

                    match.Team1 = match.Team1 == "Czechia" ? "Czech Republic" : match.Team1;
                    match.Team2 = match.Team2 == "Czechia" ? "Czech Republic" : match.Team2;


                    /** For EURO **/

                    //match.Description = detailDocument.DocumentNode.GetChildNode("span", "class", "round-name", Common.Constants.CompareModeEnum.Equal).InnerText.TrimInnerText();

                    //var scoreNode = detailDocument.DocumentNode.GetChildNode("div", "class", "match-row--flex js-match-row", Common.Constants.CompareModeEnum.Contains);

                    //match.Team1Scored = scoreNode.GetChildNode("span", "class", "js-team--home-score home-score", Common.Constants.CompareModeEnum.Equal)?.InnerText?.TrimInnerText().ToIntNullable();
                    //match.Team2Scored = scoreNode.GetChildNode("span", "class", "js-team--away-score away-score", Common.Constants.CompareModeEnum.Equal)?.InnerText?.TrimInnerText().ToIntNullable();
                    //match.Winner = (match.Team1Scored == null || match.Team1Scored == match.Team2Scored) ? null : match.Team1Scored > match.Team2Scored ? match.Team1 : match.Team2;
                    //match.Draw = match.Team1Scored != null && match.Team1Scored == match.Team2Scored ? true : null;

                    //match.Team1 = match.Team1 == "Czechia" ? "Czech Republic" : match.Team1;
                    //match.Team2 = match.Team2 == "Czechia" ? "Czech Republic" : match.Team2;

                    macthes.Add(match);
                }

                macthes = macthes.OrderBy(x => x.KickOfDate).ToList();

                //For worldcup
                for (int i = 1; i <= macthes.Count(); i++)
                {
                   if (i <= 72)
                   {
                       macthes[i-1].Description = Constant.GroupStage;
                   }
                   else if (i > 72 && i <= 88)
                   {
                       macthes[i-1].Description = Constant.RoundOf32;
                   }
                    else if (i > 88 && i <= 96)
                   {
                       macthes[i-1].Description = Constant.RoundOf16;
                   }
                   else if (i > 96 && i <= 100)
                   {
                       macthes[i-1].Description = Constant.QuaterFinal;
                   }
                   else if (i > 100 && i <= 102)
                   {
                       macthes[i-1].Description = Constant.Semi;
                   }
                   else
                   {
                       macthes[i-1].Description = Constant.Final;
                   }
                }

                //For Euro
                // for (int i = 0; i < macthes.Count(); i++)
                // {
                //     if (i <= 35)
                //     {
                //         macthes[i].Description = Constant.GroupStage;
                //     }
                //     else if (i > 35 && i <= 43)
                //     {
                //         macthes[i].Description = Constant.RoundOf16;
                //     }
                //     else if (i > 43 && i <= 47)
                //     {
                //         macthes[i].Description = Constant.QuaterFinal;
                //     }
                //     else if (i > 47 && i <= 49)
                //     {
                //         macthes[i].Description = Constant.Semi;
                //     }
                //     else
                //     {
                //         macthes[i].Description = Constant.Final;
                //     }
                // }

                await _dataInformationRepository.UpdateMatches(macthes);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static string GetMatchDetailUrl(HtmlNode matchNode)
        {
            var scoreNode = matchNode.GetChildNode("th", "class", "fscore", Common.Constants.CompareModeEnum.Equal);
            var reportUrl = scoreNode?.Descendants("a")
                .Select(node => node.GetAttributeValue("href", "").TrimInnerText())
                .FirstOrDefault(href => !string.IsNullOrWhiteSpace(href));

            if (string.IsNullOrWhiteSpace(reportUrl))
            {
                return string.Empty;
            }

            return reportUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? reportUrl
                : "https://en.wikipedia.org" + reportUrl;
        }

        private static DateTime GetUtcKickOffDate(HtmlNode matchNode)
        {
            var date = matchNode
                .GetChildNode("span", "class", "bday dtstart published updated", Common.Constants.CompareModeEnum.Contains)
                .InnerText
                .TrimInnerText();

            var timeNode = matchNode.GetChildNode("div", "class", "ftime", Common.Constants.CompareModeEnum.Equal);
            var time = GetKickOffTime(timeNode);
            var sourceUtcOffset = GetSourceUtcOffset(timeNode);

            var sourceKickOffDate = DateTime.ParseExact(
                $"{date} {time}",
                new[] { "yyyy-MM-dd h:mm tt", "yyyy-MM-dd h:mmtt", "yyyy-MM-dd HH:mm" },
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces);

            return new DateTimeOffset(sourceKickOffDate, sourceUtcOffset).UtcDateTime;
        }

        private static string GetKickOffTime(HtmlNode timeNode)
        {
            var timeText = NormalizeWikipediaText(timeNode?.InnerText);
            var utcIndex = timeText.IndexOf("UTC", StringComparison.OrdinalIgnoreCase);
            if (utcIndex >= 0)
            {
                timeText = timeText.Substring(0, utcIndex).Trim();
            }

            return timeText
                .Replace("a.m.", "AM", StringComparison.OrdinalIgnoreCase)
                .Replace("p.m.", "PM", StringComparison.OrdinalIgnoreCase)
                .Replace("a.m", "AM", StringComparison.OrdinalIgnoreCase)
                .Replace("p.m", "PM", StringComparison.OrdinalIgnoreCase)
                .Trim();
        }

        private static TimeSpan GetSourceUtcOffset(HtmlNode timeNode)
        {
            var offsetText = timeNode?.Descendants("a")
                .Select(node => node.GetAttributeValue("title", ""))
                .FirstOrDefault(title => title.Contains("UTC", StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(offsetText))
            {
                offsetText = timeNode?.InnerText;
            }

            offsetText = NormalizeWikipediaText(offsetText);
            var match = System.Text.RegularExpressions.Regex.Match(offsetText, @"UTC\s*([+\-−])\s*(\d{1,2})(?::(\d{2}))?");
            if (!match.Success)
            {
                return TimeSpan.Zero;
            }

            var sign = match.Groups[1].Value == "+" ? 1 : -1;
            var hours = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            var minutes = match.Groups[3].Success
                ? int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture)
                : 0;

            return new TimeSpan(sign * hours, sign * minutes, 0);
        }

        private static string NormalizeWikipediaText(string text)
        {
            return (text ?? string.Empty)
                .Replace('\u00A0', ' ')
                .Replace("&#160;", " ")
                .Replace("&#160", " ")
                .Replace("−", "-")
                .Trim();
        }
    }
}
