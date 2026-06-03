using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace BehindAGirl.HubConfig
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, OnlineUserConnection> OnlineConnections = new ConcurrentDictionary<string, OnlineUserConnection>();

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var userName = httpContext?.Request.Query["userName"].ToString();
            var avatar = httpContext?.Request.Query["avatar"].ToString();

            if (string.IsNullOrWhiteSpace(userName))
            {
                userName = Context.User?.Identity?.Name;
            }

            if (!string.IsNullOrWhiteSpace(userName))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userName);
                OnlineConnections[Context.ConnectionId] = new OnlineUserConnection
                {
                    UserName = userName,
                    Avatar = avatar
                };
            }

            await BroadcastOnlineUsers();
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            OnlineConnections.TryRemove(Context.ConnectionId, out var onlineUser);
            var userName = onlineUser?.UserName ?? Context.User?.Identity?.Name;

            if (!string.IsNullOrWhiteSpace(userName))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userName);
            }

            await BroadcastOnlineUsers();
            await base.OnDisconnectedAsync(ex);
        }

        private Task BroadcastOnlineUsers()
        {
            var users = OnlineConnections.Values
                .Where(x => !string.IsNullOrWhiteSpace(x.UserName))
                .GroupBy(x => x.UserName)
                .Select(x => new
                {
                    userName = x.Key,
                    avatar = x.FirstOrDefault(y => !string.IsNullOrWhiteSpace(y.Avatar))?.Avatar
                })
                .OrderBy(x => x.userName)
                .ToList();

            return Clients.All.SendAsync("broadcastonlineusers", users);
        }

        private class OnlineUserConnection
        {
            public string UserName { get; set; }
            public string Avatar { get; set; }
        }
    }
}
