using System.ComponentModel.DataAnnotations;

namespace CoffeeHouseAPI.DTOs.Order
{
    public class CreateOrderCartDTO
    {
        [Required]
        public List<int> CartIds { get; set; } = null!;

        public int? VoucherId { get; set; }

        [Required]
        public int AddressId { get; set; }
    }
}
