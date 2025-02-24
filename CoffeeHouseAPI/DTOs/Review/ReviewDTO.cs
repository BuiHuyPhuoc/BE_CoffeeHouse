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
}
