using CoffeeHouseAPI.DTOs.Topping;

namespace CoffeeHouseAPI.DTOs.Cart
{
    public class CartRequestDTO
    {
        public int ProductSizeId { get; set; }

        public int Quantity { get; set; } = 1;

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public List<ToppingOrderDTO> Toppings { get; set; } = new List<ToppingOrderDTO>();
    }
}
