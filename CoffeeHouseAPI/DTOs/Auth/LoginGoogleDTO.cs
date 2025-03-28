using System.ComponentModel.DataAnnotations;

namespace CoffeeHouseAPI.DTOs.Auth
{
    public class LoginGoogleDTO
    {
        [Required]
        public string Token { get; set; } = null!;
    }
}
