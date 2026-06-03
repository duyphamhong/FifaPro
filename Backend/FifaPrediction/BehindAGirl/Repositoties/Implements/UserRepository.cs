using BehindAGirl.Data;
using BehindAGirl.Models;
using BehindAGirl.Repositoties.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BehindAGirl.Repositoties.Implements
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddUserAddtionInfo()
        {
            try
            {
                var users = _context.AspNetUsers.Where(x=>x.UserName != "admin");

                var currentAdditons = _context.UserAddtionalInformations;

                var newAdditionUser = new List<UserAddtionalInformation>();
                foreach (var user in users)
                {
                    if(!currentAdditons.Any(x=>x.UserName == user.UserName))
                    {
                        newAdditionUser.Add(new UserAddtionalInformation
                        {
                            Id = Guid.NewGuid(),
                            CurrentCoins = 0,
                            Deposit = 0,
                            UserName = user.UserName,
                            AvatarUrl = "https://www.w3schools.com/w3css/img_avatar2.png",
                            IsPasswordChanged = false
                        });
                    }
                }

                await _context.UserAddtionalInformations.AddRangeAsync(newAdditionUser);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

		public async Task<List<UserAddtionalInformation>> GetUserAdditionInfos()
		{
			try
			{
                return _context.UserAddtionalInformations.ToList();
			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task UpdateUserAdditionInfo(List<UserAddtionalInformation> userAddtionalInformation)
        {
			try
			{
                _context.UserAddtionalInformations.UpdateRange(userAddtionalInformation);
                await _context.SaveChangesAsync();
			}
			catch (Exception)
			{

				throw;
			}
        }

        public async Task<bool> SetChampion(ChampionModel model)
        {
			try
			{
                if(_context.Matches.Any(x => x.KickOfDate < DateTime.UtcNow))
				{
                    return false;
				}

                var userAddInfo = _context.UserAddtionalInformations.FirstOrDefault(x => x.UserName == model.UserName);
                userAddInfo.ChampionPredicted = model.ChampionName;
                userAddInfo.ChampionPredictedDate = DateTime.UtcNow;
                userAddInfo.SamePredictionCount = model.SamePredictionCount;

                _context.UserAddtionalInformations.Update(userAddInfo);
                await _context.SaveChangesAsync();
                return true;
            }
			catch (Exception e)
			{

				throw;
			}
        }
    }
}
