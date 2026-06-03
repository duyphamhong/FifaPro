using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Who.Common.Constants;

namespace Who.Models
{
    public class LockUserModel
    {
        public string UserName { get; set; }
        public string UserId { get; set; }
        public LockReasonEnum LockReason { get; set; }
    }
}
