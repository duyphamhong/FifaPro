using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Crawlers
{
	public abstract class BaseCrawler<T>
	{
		protected string[] CrawUrl { get; set; }
		protected HtmlWeb Web { get; set; }
		public BaseCrawler()
		{
			Web = new HtmlWeb();
		}

		public abstract Task<T> DoCrawl();
	}
}
