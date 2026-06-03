using BehindAGirl.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BehindAGirl.HubConfig
{
	public class ChatHub: Hub<IChatHub>
	{
        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, Context.User.Identity.Name);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, Context.User.Identity.Name);
            await base.OnDisconnectedAsync(ex);
        }
    }
}
