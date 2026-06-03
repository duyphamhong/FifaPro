using BehindAGirl.Common.Constants;
using BehindAGirl.Common.Helper;
using BehindAGirl.Data;
using BehindAGirl.Models;
using BehindAGirl.Repositoties.Interfaces;
using BehindAGirl.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BehindAGirl.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IDataInformationRepository _dataInformationRepository;
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository, IDataInformationRepository dataInformationRepository)
        {
            _userRepository = userRepository;
            _dataInformationRepository = dataInformationRepository;
        }

        public Task AddUserAddtionInfo()
        {
            return _userRepository.AddUserAddtionInfo();
        }

        public async Task<CurrentPositionModel> GetPlayerPosition(string userName)
        {
            var playerPosistion = new CurrentPositionModel();
            var userAddtionInfors = await _dataInformationRepository.GetUserAddtionInformations();
            var lastMatch = await _dataInformationRepository.GetPreviousMatch();
            var predictions = await _dataInformationRepository.GetPredictions();
            var previousMatch = await _dataInformationRepository.GetPreviousMatches();

            if (lastMatch != null)
            {
                playerPosistion.TotalUserPredictedLastmatch = predictions.Count(x => x.MatchId == lastMatch.Id);
                playerPosistion.ExactPredictPercentage = predictions.Count(x => x.MatchId == lastMatch.Id) == 0 ? 0 : predictions.Count(x => x.MatchId == lastMatch.Id && x.WonType.Value == (int)WonType.WinnerAndScore) * 100 / predictions.Count(x => x.MatchId == lastMatch.Id);
                playerPosistion.FailPredictPercentage = predictions.Count(x => x.MatchId == lastMatch.Id) == 0 ? 0 : predictions.Count(x => x.MatchId == lastMatch.Id && x.WonType.Value == (int)WonType.Lose) * 100 / predictions.Count(x => x.MatchId == lastMatch.Id);
                playerPosistion.MaxPointCanReach = CalculatorHelper.CalculateMaxPointCanReach(previousMatch.Select(x => x.Description).ToList());

            }

            playerPosistion.TotalUsers = userAddtionInfors.Count;
            playerPosistion.CurrentRank = userAddtionInfors.FindIndex(x => x.UserName == userName) + 1;
            playerPosistion.Top3HighestUsers = userAddtionInfors.Where(a => a.Deposit > 0).Take(500).Select(x => new TopHighestUser
            {
                Name = x.UserName,
                CurrentPoint = x.CurrentCoins,
                AvatarUrl = x.AvatarUrl
            }).ToList();

            playerPosistion.TopHighestFreeUsers = userAddtionInfors.Where(a => a.Deposit <= 0).Take(500).Select(x => new TopHighestUser
            {
                Name = x.UserName,
                CurrentPoint = x.CurrentCoins,
                AvatarUrl = x.AvatarUrl
            }).ToList();

            return playerPosistion;
        }

        public Task UpdateUserAdditionInfo(List<UserAddtionalInformation> userAddtionalInformations)
        {
            return _userRepository.UpdateUserAdditionInfo(userAddtionalInformations);
        }

        public Task<bool> UpdateAvatar(string userName, string avatarUrl)
        {
            return _userRepository.UpdateAvatar(userName, avatarUrl);
        }

        public async Task<UserInformationModel> GetPlayerInformation(string userName)
        {
            var userAddtionInfor = await _dataInformationRepository.GetUserAddtionInformations();
            var predictions = await _dataInformationRepository.GetPredictions();
            var teams = await _dataInformationRepository.GetAllTeams();

            var userInfo = userAddtionInfor.FirstOrDefault(x => x.UserName == userName);

            if (userInfo == null)
            {
                return null;
            }
            
            var userInfoModel = new UserInformationModel
            {
                AvatarUrl = userInfo.AvatarUrl,
                UserName = userName,
                ChampionPredicted = userInfo.ChampionPredicted,
                ChampionPredictedFlag = teams.FirstOrDefault(x => x.Name == userInfo.ChampionPredicted)?.LogoUrl,
                PredictedNumber = predictions.Count(x => x.UserName == userName),
                WinnerAndScoredPredicted = predictions.Count(x => x.UserName == userName && x.WonType == (int)WonType.WinnerAndScore),
                WinnerPredicted = predictions.Count(x => x.UserName == userName && x.WonType == (int)WonType.Winner),
                LosePredicted = predictions.Count(x => x.UserName == userName && x.WonType == (int)WonType.Lose),
                CurrentCoins = userInfo.CurrentCoins,
                IsPasswordChanged = userInfo.IsPasswordChanged,
                CanChangeChampion = DateTime.UtcNow < new DateTime(2021, 06, 11, 19, 0, 0, DateTimeKind.Utc),
                SamePredictionCount = userInfo.SamePredictionCount
            };

            return userInfoModel;
        }

        public async Task<bool> SetChampion(ChampionModel model)
        {
            return await _userRepository.SetChampion(model);
        }

        public async Task<List<PredictionHistory>> GetPredictionHistory(string userName)
        {
            var predictions = (await _dataInformationRepository.GetPredictions()).Where(x => x.UserName == userName);

            var matches = await _dataInformationRepository.GetAllMatches();
            predictions = predictions.Where(x => matches.FirstOrDefault(m => m.Id == x.MatchId).KickOfDate < System.DateTime.UtcNow);

            var histories = predictions.Select(x => new PredictionHistory
            {
                Team1 = matches.FirstOrDefault(m => m.Id == x.MatchId).Team1,
                Team1Flag = matches.FirstOrDefault(m => m.Id == x.MatchId).Team1Flag,
                Team2 = matches.FirstOrDefault(m => m.Id == x.MatchId).Team2,
                Team2Flag = matches.FirstOrDefault(m => m.Id == x.MatchId).Team2Flag,
                KickOfDate = matches.FirstOrDefault(m => m.Id == x.MatchId).KickOfDate,
                PredictedDate = x.PredictedDate,
                PredictResult = x.WonType == null ? null : Enum.GetName(typeof(WonType), x.WonType)
            }).OrderByDescending(x => x.KickOfDate).ToList();

            return histories;
        }
    }
}
