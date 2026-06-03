using BehindAGirl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BehindAGirl.HubConfig
{
	public interface IChatHub
	{
		Task BroadcastMessage(ChatModel model);
	}
}
