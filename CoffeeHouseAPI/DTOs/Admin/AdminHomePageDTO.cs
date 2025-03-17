using Microsoft.Identity.Client;

namespace CoffeeHouseAPI.DTOs.Admin
{
    public class AdminHomePageDTO
    {
        public int? TotalUsers { get; set; }
        public int? TotalProducts { get; set; }
        public int? TotalOrders { get; set; }
        public int? TotalCompletedOrders { get; set; }
        public int? TotalPendingOrders { get; set; }
        public int? TotalCancelledOrders { get; set; }
        public decimal? TotalRevenue { get; set; }
        public int? CurrentVoucherCount { get; set; }
    }
}
