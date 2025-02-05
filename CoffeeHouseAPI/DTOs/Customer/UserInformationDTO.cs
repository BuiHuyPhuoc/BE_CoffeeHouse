using CoffeeHouseLib.Models;

namespace CoffeeHouseAPI.DTOs.Customer
{
    public class UserInformationDTO
    {
        public string FullName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public bool Role {  get; set; }
        public int Id { get; set; }
        public int? OrderedCount { get; set; }
    }
}
