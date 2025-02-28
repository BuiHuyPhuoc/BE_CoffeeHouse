using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CoffeeHouseAPI.DTOs.Order
{
    public class UpdateStatusOrderDTO
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public string StatusCode { get; set; } = null!;
    }
}
