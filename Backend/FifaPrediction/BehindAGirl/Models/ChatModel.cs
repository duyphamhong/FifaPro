using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BehindAGirl.Models
{
	public class ChatModel
	{
		public Guid MatchId { get; set; }
		public string UserName { get; set; }
		public string Avatar { get; set; }
		public string Content { get; set; }
		public DateTime CreatedDate { get; set; }
	}
}
