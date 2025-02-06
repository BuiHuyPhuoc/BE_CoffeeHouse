namespace CoffeeHouseAPI.DTOs.Product
{
    public class ProductHomePageDTO
    {
        public List<ProductResponseDTO> Trendings { get; set; } = new List<ProductResponseDTO>();
        public List<ProductResponseDTO> Recommends { get; set; } = new List<ProductResponseDTO>();
        public List<ProductResponseDTO> Sales { get; set; } = new List<ProductResponseDTO>();
    }
}
