using BehindAGirl.Data;
using BehindAGirl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BehindAGirl.Services.Interfaces
{
    public interface IUserService
    {
        Task AddUserAddtionInfo();
        Task<CurrentPositionModel> GetPlayerPosition(string userName);
        Task UpdateUserAdditionInfo(List<UserAddtionalInformation> userAddtionalInformations);
        Task<UserInformationModel> GetPlayerInformation(string userName);
        Task<bool> UpdateAvatar(string userName, string avatarUrl);
        Task<bool> SetChampion(ChampionModel model);
		Task<List<PredictionHistory>> GetPredictionHistory(string userName);
	}
}
