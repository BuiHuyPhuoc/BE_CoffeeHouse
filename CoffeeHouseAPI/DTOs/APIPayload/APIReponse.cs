using System.ComponentModel.DataAnnotations;

namespace CoffeeHouseAPI.DTOs.APIPayload
{
    public class APIResponseBase
    {
        public APIResponseBase() { }
        [Required]
        public int Status { get; set; }
        public string Message { get; set; } = null!;
        [Required]
        public bool IsSuccess { get; set; }
        public object Value { get; set; } = null!;
    }
}
