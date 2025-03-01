using CoffeeHouseLib.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
namespace CoffeeHouseAPI.Services.HubService
{

    public class OrderHub : Hub
    {
        public async Task SendNewOrder(Order order)
        {
            Console.WriteLine($"Đang gửi đơn hàng: {order.Id}");
            await Clients.All.SendAsync("AddOrder", JsonConvert.SerializeObject(order));
        }

        public async Task NotifyNewOrder()
        {
            Console.WriteLine("Đơn hàng mới đã được tạo!");
            await Clients.All.SendAsync("OrderUpdated");
        }
    }
}
