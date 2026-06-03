using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BehindAGirl.Models
{
	public class MatchModel
	{
		public Guid Id { get; set; }

		public string Team1 { get;set; }
		public string Team1Flag { get;set; }
		public int? Team1Scored { get;set; }
		public int? Team1ScoredPredicted { get;set; }
		public string Team2 { get;set; }
		public string Team2Flag { get;set; }
		public int? Team2Scored { get;set; }
		public int? Team2ScoredPredicted { get;set; }

		public DateTime KickOfDate { get; set; }
		public bool IsPassed { get; set; }
		public bool IsHappening { get; set; }
		public string Description { get; set; }
		public string Stadium { get; set; }
		public string DetailUrl { get; set; }
		public string Name { get; set; }

	}
}
