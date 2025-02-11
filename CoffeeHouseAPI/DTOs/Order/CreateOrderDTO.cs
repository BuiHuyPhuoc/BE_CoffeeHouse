using CoffeeHouseAPI.DTOs.Topping;
using System.ComponentModel.DataAnnotations;

namespace CoffeeHouseAPI.DTOs.Order
{
    public class CreateOrderDTO
    {
        public int? VoucherId { get; set; }

        public required List<CreateOrderDetailDTO> OrderDetails { get; set; }

        public int AddressId { get; set; }
   
    }

    public class CreateOrderDetailDTO
    {
        public int ProductSizeId { get; set; }

        public int Quantity { get; set; }

        public string? Note { get; set; }

        public List<ToppingOrderDTO>? Toppings { get; set; } = new List<ToppingOrderDTO>();
    }
}
