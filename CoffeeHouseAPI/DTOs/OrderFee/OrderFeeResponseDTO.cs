namespace CoffeeHouseAPI.DTOs.OrderFee
{
    public class OrderFeeResponseDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string FeeType { get; set; } = null!;

        public decimal Amount { get; set; }

        public bool IsActive { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
