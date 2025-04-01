using CoffeeHouseAPI.DTOs.Image;

namespace CoffeeHouseAPI.DTOs.Customer
{
    public class CustomerDTO
    {
        public int Id { get; set; }

        public string FullName { get; set; } = null!;

        public DateTime DateOfBirth { get; set; }

        public string Phone { get; set; } = null!;

        public bool IdRole { get; set; }

        public string Email { get; set; } = null!;

        public bool IsValid { get; set; }

        public ImageResponseDTO? AvatarImageNavigation { get; set; }
    }

    public class CustomerInforDTO : CustomerDTO
    {
        public DateTime? VerifyTime { get; set; }
        public DateTime? BlockExpire { get; set; }

    }
}
