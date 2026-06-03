using Bot.Extensions;
using Bot.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Bot.Crawlers
{
	public class FifaRankingCrawler : BaseCrawler<FifaRanking>
	{
		private string FifaUrl = "http://en.fifaranking.net/ranking/";
		public FifaRankingCrawler() : base()
		{
			CrawUrl = new[] { FifaUrl };
		}

		public override async Task<FifaRanking> DoCrawl()
		{
			try
			{
				var fifaRankingModel = new FifaRanking { UpdatedDate = DateTime.UtcNow };
				fifaRankingModel.TeamRankings = new List<TeamRanking>();

				var htmlDocument = await Web.LoadFromWebAsync(CrawUrl[0]);

				var tables = htmlDocument.DocumentNode.GetChildNodeList("table", "class", "table table-striped table-condensed", CompareModeEnum.Contains);

				foreach (var table in tables)
				{
					var trList = table.Descendants("tbody").FirstOrDefault().Descendants("tr").ToList();
					foreach (var tr in trList)
					{
						var tdList = tr.Descendants("td").ToList();
						fifaRankingModel.TeamRankings.Add(new TeamRanking
						{
							Rank = tdList[0].InnerText.ToInt(),
							Point = tdList[2].InnerText.ToFloat(),
							Name = tdList[3].InnerText.TrimInnerText()
						});
					}
				}

				string json = JsonSerializer.Serialize(fifaRankingModel);
				string directory = Directory.GetCurrentDirectory();
				File.WriteAllText(@$"{directory}\BotData\{typeof(FifaRanking).Name}.json", json);

				return fifaRankingModel;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}
	}
}
