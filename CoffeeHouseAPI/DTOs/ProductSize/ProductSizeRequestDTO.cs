namespace CoffeeHouseAPI.DTOs.ProductSize
{
    public class ProductSizeRequestDTO
    {
        public string Size { get; set; } = null!;

        public decimal Price { get; set; }

        public bool IsValid { get; set; } = true;
    }
}
