namespace CoffeeHouseAPI.DTOs.AI
{
    public class AIProduct
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Material { get; set; } = null!;
    }
}
