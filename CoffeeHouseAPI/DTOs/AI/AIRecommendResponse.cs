namespace CoffeeHouseAPI.DTOs.AI
{
    public class AIRecommendResponse
    {
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string Message { get; set; } = null!;
        public bool IsRelated { get; set; }
    }
}
