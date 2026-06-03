using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Who.Common.Constants;
using Who.Data;
using Who.Models;
using Who.Services.Interfaces;

namespace Who.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationUser> GetUser(string id)
        {
            ApplicationUser user = await _context.Users.FindAsync(id);
            return user;
        }

        public async Task<bool> SetPasswordChanged(string id)
        {
            var user = _context.Users.First<ApplicationUser>(x => x.Id == id);
            user.IsPasswordChanged = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetUserStatus(string id, StatusEnum status)
        {
            var user = _context.Users.First<ApplicationUser>(x => x.Id == id);
            user.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
