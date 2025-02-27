using CoffeeHouseLib.Models;

namespace CoffeeHouseAPI.Services.OrderService
{
    public interface IOrderService
    {
        List<Order> GetAllOrderWithRelate(bool isNoTracking = false);
    }
}
