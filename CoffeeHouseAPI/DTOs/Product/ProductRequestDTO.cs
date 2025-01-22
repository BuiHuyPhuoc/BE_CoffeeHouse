using CoffeeHouseAPI.DTOs.Category;
using CoffeeHouseAPI.DTOs.Image;
using CoffeeHouseAPI.DTOs.ProductSize;
using CoffeeHouseLib.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace CoffeeHouseAPI.DTOs.Product
{
    public class ProductModel
    {
        public int? Id { get; set; } = null!;
        public string ProductName { get; set; } = null!;

        public string Description { get; set; } = null!;

        public int CategoryId { get; set; }

        public bool IsValid { get; set; } = true;
        

    }
    public class ProductRequestDTO : ProductModel
    {
        public List<ProductSizeRequestDTO> ProductSizes { get; set; } = new List<ProductSizeRequestDTO>();

        public List<ImageRequestDTO> Images { get; set; } = new List<ImageRequestDTO>();
        public ImageRequestDTO ImageDefaultNavigation { get; set; } = null!;

    }

    public class ProductResponseDTO : ProductModel
    {
        public List<ProductSizeRequestDTO> ProductSizes { get; set; } = new List<ProductSizeRequestDTO>();

        public List<ImageResponseDTO> Images { get; set; } = new List<ImageResponseDTO>();
        public CategoryResponseDTO Category { get; set; } = null!;
        public ImageResponseDTO ImageDefaultNavigation { get; set; } = null!;
    }
}
