using CoffeeHouseAPI.DTOs.Topping;

namespace CoffeeHouseAPI.DTOs.Cart
{
    public class CartDetailDTO
    {
        public int Id { get; set; }

        public int? CartId { get; set; }

        public int? Quantity { get; set; }

        public ToppingDTO? Topping { get; set; }
    }
}
