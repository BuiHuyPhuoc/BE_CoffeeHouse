using CoffeeHouseAPI.DTOs.Topping;
using CoffeeHouseLib.Models;

namespace CoffeeHouseAPI.DTOs.Order
{
    public class OrderToppingDTO
    {
        public int ToppingId { get; set; }

        public int Quantity { get; set; }

        public int OrderDetailId { get; set; }

        public ToppingDTO Topping { get; set; } = null!;
    }
}
