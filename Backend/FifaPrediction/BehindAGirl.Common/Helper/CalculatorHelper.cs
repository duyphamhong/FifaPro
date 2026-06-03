using BehindAGirl.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BehindAGirl.Common.Helper
{
	public static class CalculatorHelper
	{
		public static int CalculateMaxPointCanReach(List<string> listMatchType)
		{
			return Constant.PointConverter[Constant.GroupStage][2] * listMatchType.Count(x => x == Constant.GroupStage)
				+ Constant.PointConverter[Constant.RoundOf16][2] * listMatchType.Count(x => x == Constant.RoundOf16)
				+ Constant.PointConverter[Constant.QuaterFinal][2] * listMatchType.Count(x => x == Constant.QuaterFinal)
				+ Constant.PointConverter[Constant.Semi][2] * listMatchType.Count(x => x == Constant.Semi)
				+ Constant.PointConverter[Constant.Final][2] * listMatchType.Count(x => x == Constant.Final);
		}
	}
}
