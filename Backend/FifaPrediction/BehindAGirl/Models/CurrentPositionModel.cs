using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BehindAGirl.Models
{
    public class CurrentPositionModel
    {
        public int TotalUsers { get; set; }
        public int TotalUserPredictedLastmatch { get; set; }
        public float ExactPredictPercentage { get; set; }
        public float FailPredictPercentage { get; set; }
        public int MaxPointCanReach { get; set; }
        public int CurrentRank { get; set; }
        public List<TopHighestUser> Top3HighestUsers { get; set; }
        public List<TopHighestUser> TopHighestFreeUsers { get; set; }

    }

    public class TopHighestUser
    {
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public int CurrentPoint { get; set; }
    }
}
