using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace LunchOrderingSystem.Server.Hubs
{
    public class MenuHub : Hub
    {
        public async Task SendMessage(string userId)
        {
            await Clients.All.SendAsync("ReceiveMessage", userId);
        }
    }
}
