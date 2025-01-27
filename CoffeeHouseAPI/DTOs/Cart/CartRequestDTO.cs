namespace CoffeeHouseAPI.DTOs.Cart
{
    public class CartRequestDTO
    {
        public int ProductSizeId { get; set; }

        public string? Quantity { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
