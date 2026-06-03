using BehindAGirl.Data;
using BehindAGirl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BehindAGirl.Repositoties.Interfaces
{
    public interface IUserRepository
    {
        Task UpdateUserAdditionInfo(List<UserAddtionalInformation> userAddtionalInformations);
        Task AddUserAddtionInfo();
        Task<List<UserAddtionalInformation>> GetUserAdditionInfos();
		Task<bool> SetChampion(ChampionModel model);
	}
}
