using CoffeeHouseAPI.DTOs.Image;
using CoffeeHouseAPI.DTOs.Product;
using CoffeeHouseAPI.DTOs.Topping;
using CoffeeHouseLib.Models;

namespace CoffeeHouseAPI.DTOs.Cart
{
    public class CartResponseDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int ProductSizeId { get; set; }
        public string ProductName { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public string ProductSizeName { get; set; } = null!;
        public decimal Price { get; set; }
        public ImageResponseDTO ImageDefaultNavigation { get; set; } = null!;
        public List<ToppingDTO> CartDetails { get; set; } = new List<ToppingDTO>();
        public int CartId { get; set; }
    }
}
