using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Models
{
	public class FifaRanking
	{
		public DateTime UpdatedDate { get; set; }
		public List<TeamRanking> TeamRankings { get; set; }
	}

	public class TeamRanking
	{
		public string Name { get; set; }
		public float Point { get; set; }
		public int Rank { get; set; }
	}
}
