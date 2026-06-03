using BehindAGirl.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BehindAGirl.Repositoties.Interfaces
{
    public interface IDataInformationRepository
    {
        Task<bool> SetPrediction(Prediction prediction);
        Task<bool> UpdateStandings(List<Group> groups);
        Task<bool> UpdateMatches(List<Match> matches);
        Task<Match> GetNextMatch();
        Task<Match> GetPreviousMatch(bool isGetForUpdate = false);
        Task<List<UserAddtionalInformation>> GetUserAddtionInformations();
        Task<List<Prediction>> GetPredictions();
        Task<List<Prediction>> GetPredictionsByMatchId(Guid matchId);
        Task<bool> UpdatePreviousMatches(Match match);
        Task<List<Match>> GetPreviousMatchesForUpdate();
        Task<List<Match>> GetPreviousMatches();
        Task<List<Match>> GetAllMatches();
        Task<List<Team>> GetAllTeams();

    }
}
