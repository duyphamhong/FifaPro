using BehindAGirl.Common.Constants;
using BehindAGirl.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BehindAGirl.Repositoties.Interfaces
{
    public class DataInformationRepository : IDataInformationRepository
    {
        private readonly ApplicationDbContext _context;

        public DataInformationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SetPrediction(Prediction prediction)
        {
            try
            {
                var match = _context.Matches.FirstOrDefault(x => x.Id == prediction.MatchId);
                if (match.KickOfDate < DateTime.UtcNow)
                {
                    return false;
                }

                var existingPredict = _context.Predictions.FirstOrDefault(x => x.UserName == prediction.UserName && x.MatchId == prediction.MatchId);

                if (existingPredict != null)
                {
                    var entity = _context.Predictions.Find(existingPredict.Id);
                    prediction.Id = existingPredict.Id;

                    _context.Entry(entity).CurrentValues.SetValues(prediction);
                }
                else
                {
                    _context.Predictions.Add(prediction);
                }
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdateMatches(List<Match> matches)
        {
            try
            {
                //Add groups
                var existingMatches = _context.Matches;
                var newMatches = new List<Match>();

                foreach (var match in matches)
                {
                    if (!IsExistingMatch(match))
                    {
                        match.Team1Flag = _context.Teams.FirstOrDefault(x => x.Name == match.Team1)?.LogoUrl;
                        match.Team2Flag = _context.Teams.FirstOrDefault(x => x.Name == match.Team2)?.LogoUrl;
                        newMatches.Add(match);
                    }
                    else
                    {
                        var entity = _context.Matches.Find(existingMatches.FirstOrDefault(x => (x.KickOfDate == match.KickOfDate)
                                                    && x.Description == match.Description
                                                    && ((x.Team1 == match.Team1 && x.Team2 == match.Team2)
                                                        || x.Description == Constant.RoundOf16
                                                        || x.Description == Constant.QuaterFinal
                                                        || x.Description == Constant.Semi
                                                        || x.Description == Constant.Final)).Id);
                        match.Id = existingMatches.FirstOrDefault(x => (x.KickOfDate == match.KickOfDate)
                                                    && x.Description == match.Description
                                                    && ((x.Team1 == match.Team1 && x.Team2 == match.Team2)
                                                        || x.Description == Constant.RoundOf16
                                                        || x.Description == Constant.QuaterFinal
                                                        || x.Description == Constant.Semi
                                                        || x.Description == Constant.Final)).Id;
                        match.Team1Flag = _context.Teams.FirstOrDefault(x => x.Name == match.Team1)?.LogoUrl;
                        match.Team2Flag = _context.Teams.FirstOrDefault(x => x.Name == match.Team2)?.LogoUrl;

                        _context.Entry(entity).CurrentValues.SetValues(match);
                    }
                }

                if (newMatches.Any())
                {
                    _context.Matches.AddRange(newMatches);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdateStandings(List<Group> groups)
        {
            try
            {
                //Add groups
                var existingGroups = _context.Groups;
                var newGroups = new List<Group>();

                //Add teams
                var existingTeams = _context.Teams;
                var newTeams = new List<Team>();

                foreach (var group in groups)
                {
                    if (!existingGroups.Any(x => x.Name == group.Name))
                    {
                        newGroups.Add(group);
                    }

                    foreach (var team in group.Teams)
                    {
                        if (!existingTeams.Any(x => x.Name == team.Name))
                        {
                            newTeams.Add(team);
                        }
                        else
                        {
                            var entity = _context.Teams.Find(existingTeams.FirstOrDefault(x => x.Name == team.Name).Id);
                            team.Id = existingTeams.FirstOrDefault(x => x.Name == team.Name).Id;
                            _context.Entry(entity).CurrentValues.SetValues(team);
                        }
                    }
                }


                if (newGroups.Any())
                {
                    _context.Groups.AddRange(newGroups);
                }

                if (newTeams.Any())
                {
                    _context.Teams.AddRange(newTeams);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Match> GetNextMatch()
        {
            try
            {
                var nextMatch = _context.Matches.Where(x => x.KickOfDate >= DateTime.UtcNow).OrderBy(x => x.KickOfDate).FirstOrDefault();
                return nextMatch;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<UserAddtionalInformation>> GetUserAddtionInformations()
        {
            try
            {
                var result = (from p in _context.UserAddtionalInformations
                              join e in _context.AspNetUsers
                                           on p.UserName equals e.UserName
                              orderby p.CurrentCoins descending
                              where e.UserName != "thewinner"
                              select new UserAddtionalInformation
                              {
                                  Id = p.Id,
                                  UserName = p.UserName,
                                  CurrentCoins = p.CurrentCoins,
                                  Deposit = p.Deposit,
                                  AvatarUrl = p.AvatarUrl,
                                  ChampionPredicted = p.ChampionPredicted,
                                  IsPasswordChanged = e.IsPasswordChanged,
                                  SamePredictionCount = p.SamePredictionCount,
                                  ChampionPredictedDate = p.ChampionPredictedDate
                              }).ToList();

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Prediction>> GetPredictions()
        {
            try
            {
                return _context.Predictions.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Match> GetPreviousMatch(bool isGetForUpdate = false)
        {
            try
            {
                if (isGetForUpdate)
                {
                    return _context.Matches.Where(x => x.KickOfDate < DateTime.UtcNow && x.Winner == null && x.Draw == null).OrderByDescending(x => x.KickOfDate).FirstOrDefault();
                }
                else
                {
                    var previousMatch = _context.Matches.Where(x => x.KickOfDate < DateTime.UtcNow && (x.Winner != null || x.Draw != null)).OrderByDescending(x => x.KickOfDate).FirstOrDefault();

                    return previousMatch;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Prediction>> GetPredictionsByMatchId(Guid matchId)
        {
            try
            {
                return _context.Predictions.Where(x => x.MatchId == matchId).ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> UpdatePreviousMatches(Match match)
        {
            try
            {
                var entity = _context.Matches.Find(match.Id);
                _context.Entry(entity).CurrentValues.SetValues(match);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Match>> GetPreviousMatches()
        {
            try
            {
                return _context.Matches.Where(x => x.KickOfDate < DateTime.UtcNow).ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Match>> GetAllMatches()
        {
            try
            {
                return _context.Matches.OrderBy(x => x.KickOfDate).ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Team>> GetAllTeams()
        {
            return _context.Teams.ToList();
        }

        private bool IsExistingMatch(Match match)
        {
            return _context.Matches.Any(x => (x.KickOfDate == match.KickOfDate)
                                                    && x.Description == match.Description
                                                    && ((x.Team1 == match.Team1 && x.Team2 == match.Team2)
                                                        || x.Description == Constant.RoundOf16 
                                                        || x.Description == Constant.QuaterFinal 
                                                        || x.Description == Constant.Semi 
                                                        || x.Description == Constant.Final)
                                        );
        }
    }
}
