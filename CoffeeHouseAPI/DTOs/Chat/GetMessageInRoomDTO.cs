using System.ComponentModel.DataAnnotations;

namespace CoffeeHouseAPI.DTOs.Chat
{
    public class GetMessageInRoomDTO
    {
        [Required]
        public Guid RoomId { get; set; }
        [Required]
        public int Quantity { get; set; } = 20;
    }
}
