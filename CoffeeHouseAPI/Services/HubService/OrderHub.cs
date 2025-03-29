using CoffeeHouseLib.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
namespace CoffeeHouseAPI.Services.HubService
{

    public class OrderHub : Hub
    {
        public async Task SendNewOrder(Order order)
        {
            await Clients.All.SendAsync("AddOrder", JsonConvert.SerializeObject(order));
        }

        public async Task NotifyNewOrder()
        {
            await Clients.All.SendAsync("OrderUpdated");
        }
    }
}
