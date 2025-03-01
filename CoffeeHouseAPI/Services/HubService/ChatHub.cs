using Microsoft.AspNetCore.SignalR;

namespace CoffeeHouseAPI.Services.HubService
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}