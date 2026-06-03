using Microsoft.AspNetCore.Identity;
using Who.Common.Constants;

namespace Who.Models
{
    public class ApplicationUser : IdentityUser
    {
        public StatusEnum Status { get; set; }

        public bool IsPasswordChanged { get; set; }

        public bool IsActive => Status == StatusEnum.Active;
    }
}
