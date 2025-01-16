namespace CoffeeHouseAPI.DTOs.Address
{
    public class AddressDTO
    {
        public int? Id { get; set; }

        public int? CustomerId { get; set; }

        public string Address { get; set; } = null!;

        public bool IsDefault { get; set; } = true;
    }
}
