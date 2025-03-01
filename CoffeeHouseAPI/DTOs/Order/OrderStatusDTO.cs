namespace CoffeeHouseAPI.DTOs.Order
{
    public class OrderStatusDTO
    {
        public string StatusCode { get; set; } = null!;

        public string StatusName { get; set; } = null!;

        public int Index { get; set; }
    }
}
