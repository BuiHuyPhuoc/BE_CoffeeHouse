namespace CoffeeHouseAPI.DTOs.Voucher
{
    public class VoucherDTO
    {
        public int? Id { get; set; }

        public string Code { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string DiscountType { get; set; } = null!;

        public decimal DiscountValue { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? UsageLimit { get; set; }

        public int LitmitPerUser { get; set; }

        public bool IsActive { get; set; }

        public decimal MinOrderValue { get; set; }
    }
}
