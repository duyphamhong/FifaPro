using BehindAGirl.Common.Constants;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BehindAGirl.Common.Extensions
{
	public static class Extension
	{
		public static string TrimInnerText(this string text)
		{
			return text.Replace("\r\n", "").Replace("Playing now", "").Replace("&#160", "").Replace(";", "").Trim();
		}
		public static int ToInt(this string text)
		{
			return Int16.Parse(text.Replace("\r\n", "").Trim());
		}
		public static int? ToIntNullable(this string text)
		{
			return string.IsNullOrEmpty(text) ? null : Int16.Parse(text.Replace("\r\n", "").Trim());
		}
		public static HtmlNode GetChildNode(this HtmlNode node, string elementTag, string compareAttribute, string compareValue, CompareModeEnum compareMode = CompareModeEnum.Equal)
		{
			switch (compareMode)
			{
				case CompareModeEnum.Equal:
					return node.Descendants(elementTag).FirstOrDefault(node => node.GetAttributeValue(compareAttribute, "").Equals(compareValue));
				case CompareModeEnum.Contains:
					return node.Descendants(elementTag).FirstOrDefault(node => node.GetAttributeValue(compareAttribute, "").Contains(compareValue));
				default:
					return node.Descendants(elementTag).FirstOrDefault(node => node.GetAttributeValue(compareAttribute, "").Equals(compareValue));
			}
		}

		public static List<HtmlNode> GetChildNodeList(this HtmlNode node, string elementTag, string compareAttribute, string compareValue, CompareModeEnum compareMode = CompareModeEnum.Equal)
		{
			switch (compareMode)
			{
				case CompareModeEnum.Equal:
					return node.Descendants(elementTag).Where(node => node.GetAttributeValue(compareAttribute, "").Equals(compareValue)).ToList();
				case CompareModeEnum.Contains:
					return node.Descendants(elementTag).Where(node => node.GetAttributeValue(compareAttribute, "").Contains(compareValue)).ToList();
				default:
					return node.Descendants(elementTag).Where(node => node.GetAttributeValue(compareAttribute, "").Equals(compareValue)).ToList();
			}
		}

		public static DateTime ToDateTime(this string datetime)
		{
			return DateTime.Parse(datetime);
		}

		public static int ConvertToPointCollected(this WonType type, string round)
		{
			return Constant.PointConverter[round][(int) type];
		}
	}
}
