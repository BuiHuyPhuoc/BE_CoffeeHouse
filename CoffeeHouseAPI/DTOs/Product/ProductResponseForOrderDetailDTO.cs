using CoffeeHouseAPI.DTOs.Category;
using CoffeeHouseAPI.DTOs.Discount;
using CoffeeHouseAPI.DTOs.Image;
using CoffeeHouseAPI.DTOs.ProductSize;
using CoffeeHouseAPI.DTOs.Review;
using CoffeeHouseAPI.DTOs.Topping;

namespace CoffeeHouseAPI.DTOs.Product
{
    public class ProductResponseForOrderDetailDTO
    {
        public int? Id { get; set; } = null!;
        public string ProductName { get; set; } = null!;

        public string Description { get; set; } = null!;

        public int CategoryId { get; set; }

        public bool IsValid { get; set; } = true;

        public string? Description2 { get; set; }

        public string? Material { get; set; }

        public DiscountDTO? Discount { get; set; }

        public DateTime? CreatedAt { get; set; }

        public CategoryResponseDTO Category { get; set; } = null!;

        public ImageResponseDTO ImageDefaultNavigation { get; set; } = null!;
    }
}
