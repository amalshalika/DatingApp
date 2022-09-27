using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker _presenceTracker;

        public PresenceHub(PresenceTracker presenceTracker)
        {
            _presenceTracker = presenceTracker;
        }
        public override async Task OnConnectedAsync()
        {
            bool isNewUser = await _presenceTracker.UserConnected(Context.User.GetUserName(),Context.ConnectionId);
            if(isNewUser)
                await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUserName());
            await Clients.Caller.SendAsync("GetOnlineUsers", await _presenceTracker.GetOnlineUsers());
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            bool isUserOffline = await _presenceTracker.UserDisconnected(Context.User.GetUserName(), Context.ConnectionId);
            if(isUserOffline)
                await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUserName());
            //await Clients.All.SendAsync("GetOnlineUsers", await _presenceTracker.GetOnlineUsers());

            await base.OnDisconnectedAsync(exception);
        }
    }
}
