namespace CoffeeHouseAPI.DTOs.Discount
{
    public class DiscountDTO
    {
        public int? Id { get; set; }

        public string DiscountName { get; set; } = null!;

        public string DiscountType { get; set; } = null!;

        public decimal DiscountValue { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }

    }
}
