using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BehindAGirl.Models
{
	public class UserInformationModel
	{
		public string UserName { get; set; }
		public string AvatarUrl { get; set; }
		public string ChampionPredicted { get; set; }
		public string ChampionPredictedFlag { get; set; }
		public int PredictedNumber { get; set; }
		public int WinnerAndScoredPredicted { get; set; }
		public int WinnerPredicted { get; set; }
		public int LosePredicted { get; set; }
		public int CurrentPosition { get; set; }
		public int CurrentCoins { get; set; }
        public bool IsPasswordChanged { get; set; }
        public bool CanChangeChampion { get; set; }
		public int? SamePredictionCount { get; set; }
	}
}
