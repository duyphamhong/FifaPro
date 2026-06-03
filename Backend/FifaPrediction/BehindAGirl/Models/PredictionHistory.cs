using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BehindAGirl.Models
{
	public class PredictionHistory
	{
		public string Team1 { get; set; }
		public string Team1Flag { get; set; }
		public string Team2 { get; set; }
		public string Team2Flag { get; set; }
		public DateTime KickOfDate { get; set; }
		public DateTime PredictedDate { get; set; }

		public string PredictResult { get; set; }
	}
}
