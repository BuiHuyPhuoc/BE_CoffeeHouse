using CoffeeHouseAPI.DTOs.Product;
using CoffeeHouseLib.Models;
using Microsoft.Identity.Client;

namespace CoffeeHouseAPI.DTOs.ProductSize
{
    public class ProductSizeResponseDTO
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string Size { get; set; } = null!;

        public decimal Price { get; set; }

        public bool IsValid { get; set; }

        public ProductResponseDTO Product { get; set; } = null!;
    }
}
