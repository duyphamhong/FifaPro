using BehindAGirl.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BehindAGirl.Models
{
	public class MatchPredictModel
	{
		public string Team1 { get; set; }
		public string Team1Flag { get; set; }
		public string Team2 { get; set; }
		public string Team2Flag { get; set; }
		public List<Prediction> Predictions { get; set; }
		public MatchPredictModel()
		{
			Predictions = new List<Prediction>();
		}
	}
}
