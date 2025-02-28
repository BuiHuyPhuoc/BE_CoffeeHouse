using CoffeeHouseLib.Models;
using Microsoft.AspNetCore.SignalR;
namespace CoffeeHouseAPI.HubService
{

    public class OrderHub : Hub
    {
        public async Task SendNewOrder(Order order)
        {
            await Clients.All.SendAsync("AddOrder", order);
        }
    }
}
