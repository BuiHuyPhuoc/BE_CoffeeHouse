using Microsoft.Identity.Client;

namespace CoffeeHouseAPI.DTOs.ProductSize
{
    public class ProductSizeRequestDTO
    {
        public int? Id { get; set; }
        public string Size { get; set; } = null!;

        public decimal Price { get; set; }

        public bool IsValid { get; set; } = true;
    }
}
