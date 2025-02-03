using CoffeeHouseAPI.DTOs.Topping;
using System.Text.Json.Serialization;

namespace CoffeeHouseAPI.DTOs.Cart
{
    public class CartRequestDTO
    {
        public int ProductSizeId { get; set; }

        public int Quantity { get; set; } = 1;

        [JsonIgnore]
        public DateTime? CreatedAt { get; set; }

        [JsonIgnore]
        public DateTime? UpdatedAt { get; set; }

        public List<ToppingOrderDTO> Toppings { get; set; } = new List<ToppingOrderDTO>();
    }
}
