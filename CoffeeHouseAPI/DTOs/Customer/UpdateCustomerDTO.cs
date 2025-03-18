namespace CoffeeHouseAPI.DTOs.Customer
{
    public class UpdateCustomerDTO
    {
        public string? FullName { get; set; } = null!;

        public DateTime? DateOfBirth { get; set; }

        public string? Phone { get; set; } = null!;

        public bool? IdRole { get; set; }
    }
}
