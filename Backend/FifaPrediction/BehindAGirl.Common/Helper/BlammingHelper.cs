using BehindAGirl.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BehindAGirl.Common.Helper
{
	public static class BlammingHelper
	{
		private static readonly string[] SuccessResult =
		{
			"Đoán cũng ghê đó",
			"Trúng phóc",
			"Cũng đúng phết",
			"Kẻ thắng nói gì cũng đúng"
		};

		private static readonly string[] FailResult =
		{
			"Sai bét",
			"Gáy sớm ăn ***",
			"Thua sấp mặt",
			"Toang",
			"Đi hộ đê"
		};

		private static readonly string[] HalfSuccessResult =
		{
			"Đúng có được 1 nữa",
			"Hơi đúng",
			"Cũng đúng đúng",
			"Dễ thế ai đoán chả trúng"
		};

		public static string GetBlammerResult(WonType? type)
		{
			var randomIndex = new Random().Next(0, 4);
			return type == null ? null : type == WonType.Lose ? FailResult[randomIndex] : type == WonType.WinnerAndScore ? SuccessResult[randomIndex] : HalfSuccessResult[randomIndex];
		}
	}
}
