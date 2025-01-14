using CoffeeHouseAPI.DTOs.Category;
using CoffeeHouseAPI.DTOs.Image;

namespace CoffeeHouseAPI.DTOs.Product
{
    public class ProductDTO
    {
        public int? Id { get; set; }

        public string ProductName { get; set; } = null!;

        public string Description { get; set; } = null!;

        public int CategoryId { get; set; }

        public CategoryDTO? Category { get; set; }

        public List<ImageRequestDTO> Images { get; set; } = null!;
        public decimal? MinPrice { get; set; }

        public bool IsValid { get; set; }
    }
}
