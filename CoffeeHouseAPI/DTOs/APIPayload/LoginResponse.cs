using System.ComponentModel.DataAnnotations;

namespace CoffeeHouseAPI.DTOs.APIPayload
{
    public class LoginResponse
    {
        public string FullName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public string Phone { get; set; } = null!;
        [Required(ErrorMessage = "IdRole is missing")]
        public bool IdRole { get; set; }
        public string Email { get; set; } = null!;
    }
}
