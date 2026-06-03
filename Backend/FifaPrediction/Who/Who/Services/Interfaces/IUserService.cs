using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Who.Common.Constants;
using Who.Models;

namespace Who.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApplicationUser> GetUser(string id);
        Task<bool> SetUserStatus(string id, StatusEnum status);
        Task<bool> SetPasswordChanged(string id);
    }
}
