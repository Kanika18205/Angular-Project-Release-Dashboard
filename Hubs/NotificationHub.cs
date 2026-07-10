using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ReleaseDashboard.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
    }
}