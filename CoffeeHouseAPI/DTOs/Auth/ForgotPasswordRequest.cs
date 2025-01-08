using Microsoft.AspNetCore.Antiforgery;
using System.ComponentModel.DataAnnotations;

namespace CoffeeHouseAPI.DTOs.Auth
{
    public class ForgotPasswordRequest
    {
        [Required]
        public string Otp { get; set; } = null!;
        [Required]
        public string NewPassword { get; set; } = null!;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}
