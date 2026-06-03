using System;
using System.Collections.Generic;

#nullable disable

namespace BehindAGirl.Data
{
    public partial class UserAddtionalInformation
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public int CurrentCoins { get; set; }
        public decimal Deposit { get; set; }
        public string AvatarUrl { get; set; }
        public string ChampionPredicted { get; set; }

        public bool IsPasswordChanged { get; set; }
        public DateTime? ChampionPredictedDate { get; set; }
        public int? SamePredictionCount { get; set; }
    }
}
