using BehindAGirl.Data;
using BehindAGirl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BehindAGirl.Services.Interfaces
{
    public interface IDataInformationService
    {
        Task<bool> SetPrediction(Prediction prediction);
        Task<MatchModel> GetNextMatch(string userName);
        Task<PreviousMatchModel> GetPreviousMatch();
		Task<bool> UpdatePreviousMatch(UpdatePreviousMatchModel model);
        Task<List<MatchModel>> GetMatches(string userName);
        Task<List<Team>> GetTeams();
        Task<MatchPredictModel> GetMatchPredict(string id);

    }
}
