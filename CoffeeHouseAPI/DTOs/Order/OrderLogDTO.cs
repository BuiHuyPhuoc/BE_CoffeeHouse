using CoffeeHouseLib.Models;

namespace CoffeeHouseAPI.DTOs.Order
{
    public class OrderLogDTO
    {
        public string StatusCode { get; set; } = null!;

        public int OrderId { get; set; }

        public DateTime TimeLog { get; set; }

        public OrderStatusDTO StatusCodeNavigation { get; set; } = null!;
    }
}
