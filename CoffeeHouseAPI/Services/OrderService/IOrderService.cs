using CoffeeHouseLib.Models;

namespace CoffeeHouseAPI.Services.OrderService
{
    public interface IOrderService
    {
        List<Order> GetAllOrderWithRelate(bool isNoTracking = false);

        List<Order> GetAllSuccessOrderWithRelate(bool isNoTracking = false);

        decimal GetTotalPriceAllOrder();

        decimal GetTotalPriceOrder(Order order);

        decimal GetTotalPriceOrder(List<Order> orders);
    }
}
