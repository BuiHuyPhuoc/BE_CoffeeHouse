using CoffeeHouseAPI.DTOs.Category;
using CoffeeHouseAPI.DTOs.Image;
using CoffeeHouseAPI.DTOs.ProductSize;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace CoffeeHouseAPI.DTOs.Product
{
    public class ProductModel
    {
        public string ProductName { get; set; } = null!;

        public string Description { get; set; } = null!;

        public int CategoryId { get; set; }

        public bool IsValid { get; set; } = true;
    }
    public class ProductRequestDTO : ProductModel
    {
        public List<ProductSizeRequestDTO> Sizes { get; set; } = new List<ProductSizeRequestDTO>();

        public List<ImageRequestDTO> Images { get; set; } = new List<ImageRequestDTO>();
    }

    public class ProductResponseDTO : ProductModel
    {
        public List<ProductSizeRequestDTO> Sizes { get; set; } = new List<ProductSizeRequestDTO>();

        public List<ImageResponseDTO> Images { get; set; } = new List<ImageResponseDTO>();
        public CategoryResponseDTO Category { get; set; } = null!;
    }
}
