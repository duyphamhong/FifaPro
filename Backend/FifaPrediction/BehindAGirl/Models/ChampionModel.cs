using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BehindAGirl.Models
{
	public class ChampionModel
	{
		public string ChampionName { get; set; }
		public string UserName { get; set; }
		public DateTime ChampionPredictedDate { get; set; }
		public int SamePredictionCount { get; set; }
	}
}
