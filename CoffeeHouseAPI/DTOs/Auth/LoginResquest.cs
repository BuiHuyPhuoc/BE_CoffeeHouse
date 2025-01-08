using System.ComponentModel.DataAnnotations;

namespace CoffeeHouseAPI.DTOs.Auth
{
    public class LoginResquest
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
    }
}
