using BehindAGirl.Common.Constants;
using BehindAGirl.Common.Extensions;
using BehindAGirl.Common.Helper;
using BehindAGirl.Data;
using BehindAGirl.Models;
using BehindAGirl.Repositoties.Interfaces;
using BehindAGirl.Services.Interfaces;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BehindAGirl.Services.Implements
{
	public class DataInformationService : IDataInformationService
	{
		private readonly IDataInformationRepository _dataInformationRepository;
		private readonly IUserRepository _userRepository;
		public DataInformationService(IDataInformationRepository dataInformationRepository, IUserRepository userRepository)
		{
			_dataInformationRepository = dataInformationRepository;
			_userRepository = userRepository;
		}

		public async Task<MatchModel> GetNextMatch(string userName)
		{
			var match = await _dataInformationRepository.GetNextMatch();
			if (match == null)
			{
				return null;
			}

			var prediction = (await _dataInformationRepository.GetPredictionsByMatchId(match.Id))
				.FirstOrDefault(x => x.UserName == userName);

			return ToMatchModel(match, prediction);
		}

		public async Task<PreviousMatchModel> GetPreviousMatch()
		{
			var model = new PreviousMatchModel();

			var match = await _dataInformationRepository.GetPreviousMatch();
			if (match != null)
			{
				match.KickOfDate = match.KickOfDate.ToLocalTime();
				var predictions = await _dataInformationRepository.GetPredictionsByMatchId(match.Id);

				model.Match = match;

				var predict = predictions.FirstOrDefault(x => !string.IsNullOrEmpty(x.Blammer));

				if (predict != null)
				{
					var randomIndex = new Random().Next(0, predictions.Count);
					model.Blammer = predict?.UserName;
					model.BlameContent = predict?.Blammer;
					model.BlammerResult = BlammingHelper.GetBlammerResult((WonType)(predict?.WonType));
				}
			}

			return model;
		}

		public Task<bool> SetPrediction(Prediction prediction)
		{
			prediction.PredictedDate = DateTime.UtcNow;
			prediction.Id = Guid.NewGuid();
			return _dataInformationRepository.SetPrediction(prediction);
		}

		public async Task<bool> UpdatePreviousMatch(UpdatePreviousMatchModel model)
		{
			model = model ?? new UpdatePreviousMatchModel();
			var isBulkUpdate = string.IsNullOrEmpty(model.MatchId);
			List<Match> previousMatches;
			if (isBulkUpdate)
			{
				previousMatches = await _dataInformationRepository.GetPreviousMatchesForUpdate();
			}
			else
			{
				if (!Guid.TryParse(model.MatchId, out var matchId))
				{
					return false;
				}

				var matches = await _dataInformationRepository.GetPreviousMatches();
				previousMatches = matches.Where(x => x.Id == matchId).ToList();
			}

			if (previousMatches == null || !previousMatches.Any())
			{
				return false;
			}

			var userAddtionalInfos = await _userRepository.GetUserAdditionInfos();
			var isUpdated = false;

			foreach (var previousMatch in previousMatches)
			{
				if (await TryUpdatePreviousMatch(previousMatch, model, userAddtionalInfos, isBulkUpdate))
				{
					isUpdated = true;
				}
			}

			if (isUpdated)
			{
				await _userRepository.UpdateUserAdditionInfo(userAddtionalInfos);
			}

			return isUpdated;
		}

		private async Task<bool> TryUpdatePreviousMatch(Match previousMatch, UpdatePreviousMatchModel model, List<UserAddtionalInformation> userAddtionalInfos, bool isBulkUpdate)
		{
			if (previousMatch == null)
			{
				return false;
			}

			//Crawl data
			var web = new HtmlWeb();
			var detailUrl = string.IsNullOrWhiteSpace(previousMatch.DetailUrl)
				? WorldCupMatchParser.TournamentUrl
				: previousMatch.DetailUrl;
			var detailDocument = await web.LoadFromWebAsync(detailUrl);

			/**Euro**/
			//var scoreNode = detailDocument.DocumentNode.GetChildNode("div", "class", "match-row--flex js-match-row", Common.Constants.CompareModeEnum.Contains);

			//previousMatch.Team1Scored = scoreNode.GetChildNode("span", "class", "js-team--home-score home-score", Common.Constants.CompareModeEnum.Equal)?.InnerText?.TrimInnerText().ToIntNullable();
			//previousMatch.Team2Scored = scoreNode.GetChildNode("span", "class", "js-team--away-score away-score", Common.Constants.CompareModeEnum.Equal)?.InnerText?.TrimInnerText().ToIntNullable();

			/**WorldCup**/
			var divs = detailDocument.DocumentNode.GetChildNodeList("div", "class", "footballbox", Common.Constants.CompareModeEnum.Equal);

			foreach (var div in divs)
			{
				var team1 = WorldCupMatchParser.GetHomeTeam(div);
				var team2 = WorldCupMatchParser.GetAwayTeam(div);
				var previousTeam1 = WorldCupMatchParser.NormalizeTeamName(previousMatch.Team1);
				var previousTeam2 = WorldCupMatchParser.NormalizeTeamName(previousMatch.Team2);

				if (previousTeam1 == team1 && previousTeam2 == team2 && WorldCupMatchParser.TryGetScore(div, out var team1Score, out var team2Score))
				{
					previousMatch.Team1Scored = team1Score;
					previousMatch.Team2Scored = team2Score;

					previousMatch.Winner = (previousMatch.Team1Scored == null || previousMatch.Team1Scored == previousMatch.Team2Scored) ? null : previousMatch.Team1Scored > previousMatch.Team2Scored ? previousMatch.Team1 : previousMatch.Team2;
					previousMatch.Draw = previousMatch.Team1Scored != null && previousMatch.Team1Scored == previousMatch.Team2Scored ? true : null;

					break;
				}
			}

			if (previousMatch.Team1Scored == null || previousMatch.Team2Scored == null)
			{
				return false;
			}

			if (!isBulkUpdate || !string.IsNullOrWhiteSpace(model.AfterMatchSummary))
			{
				previousMatch.AfterMatchSummary = model.AfterMatchSummary;
			}

			//Get predictions
			var predictions = await _dataInformationRepository.GetPredictionsByMatchId(previousMatch.Id);
			if (predictions.Any(x => x.WonType != null))
			{
				return false;
			}

			foreach (var item in predictions)
			{
				var wontype = GetWonType(previousMatch, item);
				item.WonType = (int?)wontype;
				item.PointCollected = wontype.ConvertToPointCollected(previousMatch.Description);

				//Update point for user
				var userAddtionalInfo = userAddtionalInfos.FirstOrDefault(x => x.UserName == item.UserName);
				if (userAddtionalInfo != null)
				{
					userAddtionalInfo.CurrentCoins += item.PointCollected.Value;
				}

				//Update prediction
				await _dataInformationRepository.SetPrediction(item);
			}

			//update db
			await _dataInformationRepository.UpdatePreviousMatches(previousMatch);

			return true;
		}

		private WonType GetWonType(Match previousMatch, Prediction prediction)
		{
			if (prediction.Team1Score == previousMatch.Team1Scored && prediction.Team2Score == previousMatch.Team2Scored)
			{
				return WonType.WinnerAndScore;
			}
			else if (previousMatch.Team1Scored == previousMatch.Team2Scored
				&& prediction.Team1Score == prediction.Team2Score
				&& prediction.Team1Score != previousMatch.Team1Scored)
			{
				return WonType.Winner;
			}
			else if ((previousMatch.Team1Scored > previousMatch.Team2Scored && prediction.Team1Score > prediction.Team2Score)
				|| (previousMatch.Team1Scored < previousMatch.Team2Scored && prediction.Team1Score < prediction.Team2Score))
			{
				return WonType.Winner;
			}
			else
			{
				return WonType.Lose;
			}
		}

		public async Task<List<MatchModel>> GetMatches(string userName)
		{
			var matches = await _dataInformationRepository.GetAllMatches();
			var predictions = await _dataInformationRepository.GetPredictions();

			var matchModels = matches.Select(x =>
				ToMatchModel(x, predictions.FirstOrDefault(p => p.UserName == userName && x.Id == p.MatchId))).ToList();

			return matchModels;
		}

		private MatchModel ToMatchModel(Match match, Prediction prediction)
		{
			var localKickOffDate = match.KickOfDate.ToLocalTime();
			var localNow = DateTime.Now;
			
			//add 2 hours to ensure the match is ended, because some match have long extra time, penalty, or delay time
			var matchEndDate = localKickOffDate.AddHours(2);

			return new MatchModel
			{
				Id = match.Id,
				Team1 = match.Team1,
				Team1Scored = match.Team1Scored,
				Team1Flag = match.Team1Flag,
				Team1ScoredPredicted = prediction?.Team1Score,
				Team2 = match.Team2,
				Team2Scored = match.Team2Scored,
				Team2Flag = match.Team2Flag,
				Team2ScoredPredicted = prediction?.Team2Score,
				KickOfDate = localKickOffDate,
				IsPassed = localNow > matchEndDate,
				IsHappening = localNow >= localKickOffDate && localNow <= matchEndDate,
				Description = match.Description,
				Stadium = match.Stadium,
				DetailUrl = match.DetailUrl,
				Name = match.Name
			};
		}

		public async Task<List<Team>> GetTeams()
		{
			return (await _dataInformationRepository.GetAllTeams()).OrderBy(x => x.Name).ToList();
		}

		public async Task<MatchPredictModel> GetMatchPredict(string id)
		{
			try
			{
				var predictions = await _dataInformationRepository.GetPredictionsByMatchId(new Guid(id));
				var match = (await _dataInformationRepository.GetAllMatches()).FirstOrDefault(x => x.Id == new Guid(id));

				foreach (var item in predictions)
				{
					item.PredictedDate = item.PredictedDate.ToLocalTime();
				}

				if (predictions != null && match != null)
				{
					MatchPredictModel model = new MatchPredictModel
					{
						Team1 = match.Team1,
						Team1Flag = match.Team1Flag,
						Team2 = match.Team2,
						Team2Flag = match.Team2Flag,
						Predictions = predictions
					};
					return model;
				}

				return null;
			}
			catch (Exception e)
			{

				throw;
			}
		}
	}
}
