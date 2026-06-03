using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BehindAGirl.Common.Constants
{
	public static class Constant
	{
		public const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

		public const string GroupStage = "Group stage";
		public const string RoundOf32 = "Round of 32";
		public const string RoundOf16 = "Round of 16";
		public const string QuaterFinal = "Quarter-finals";
		public const string Semi = "Semi-finals";
		public const string Final = "Final";

		public static Dictionary<string, List<int>> PointConverter = new Dictionary<string, List<int>>()
		{
			{ GroupStage, new List<int>{0,5,10} },
			{ RoundOf16, new List<int>{0,10,20} },
			{ QuaterFinal, new List<int>{0,10,20} },
			{ Semi, new List<int>{0,15,30} },
			{ Final, new List<int>{0,25,50} }
		};
	}
}
