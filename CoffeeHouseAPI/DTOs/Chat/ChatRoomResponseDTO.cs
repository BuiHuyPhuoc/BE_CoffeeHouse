using CoffeeHouseAPI.DTOs.Customer;
using CoffeeHouseLib.Models;

namespace CoffeeHouseAPI.DTOs.Chat
{
    public class ChatRoomResponseDTO
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public string RoomType { get; set; } = null!;

        public DateTime? CreatedAt { get; set; }

        public bool IsValid { get; set; }

        public DateTime? LastMessageAt { get; set; }

        public List<RoomUserResponseDTO> RoomUsers { get; set; } = new List<RoomUserResponseDTO>();
    }
}
