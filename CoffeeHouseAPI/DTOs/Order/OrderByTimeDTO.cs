namespace CoffeeHouseAPI.DTOs.Order
{
    public class OrderByTimeDTO
    {
        public int OrderCount { get; set; }

        public decimal Revenue { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }
    }
}
