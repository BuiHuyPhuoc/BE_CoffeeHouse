
using CoffeeHouseAPI.DTOs.APIPayload;
using Microsoft.AspNetCore.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace CoffeeHouseAPI.DTOs.Review
{
    public class ReviewDTO
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public short Rating { get; set; }

        [Required]
        public string Comment { get; set; } = null!;

    }

    public class ReviewResponseDTO : ReviewDTO
    {
        public LoginResponse Customer { get; set; } = null!;
        public DateTime UpdatedAt { get; set; }
    }
}
