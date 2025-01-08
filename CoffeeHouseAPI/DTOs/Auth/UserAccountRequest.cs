using CoffeeHouseAPI.DTOs.APIPayload;
using System.ComponentModel.DataAnnotations;

namespace CoffeeHouseAPI.DTOs.Auth
{
    public class UserAccountRequest : LoginResponse
    {
        public string Password { get; set; } = null!;
    }

    public class VerifyAccountRequest
    {
        [EmailAddress]
        public string Email { get; set; } = null!;
        public string Otp { get; set; } = null!;
    }
}
