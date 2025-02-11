namespace CoffeeHouseAPI.DTOs.Address
{
    public class AddressDTO
    {
        public int? Id { get; set; }

        public int? CustomerId { get; set; }

        public string AddressNumber { get; set; } = null!;

        public bool IsDefault { get; set; }

        public int Ward { get; set; }

        public int District { get; set; }

        public int Province { get; set; }

        public string FullName { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;
    }
}
