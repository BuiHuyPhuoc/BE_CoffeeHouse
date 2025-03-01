using CoffeeHouseAPI.DTOs.ProductSize;
using CoffeeHouseLib.Models;

namespace CoffeeHouseAPI.DTOs.Order
{
    public class OrderDetailDTO
    {
        public int Id { get; set; }

        public int ProductSizeId { get; set; }

        public int OrderId { get; set; }

        public string? Note { get; set; }

        public int? Quantity { get; set; }

        public ProductSizeResponseForOrderDetailDTO ProductSize { get; set; } = null!;

        public List<OrderToppingDTO> OrderToppings { get; set; } = new List<OrderToppingDTO>();
    }
}
