using CoffeeHouseAPI.DTOs.Customer;
using CoffeeHouseLib.Models;

namespace CoffeeHouseAPI.DTOs.Chat
{
    public class RoomUserResponseDTO
    {
        public Guid Id { get; set; }

        public Guid RoomId { get; set; }

        public int UserId { get; set; }

        public DateTime? JoinedAt { get; set; }

        public string? Role { get; set; }

        public bool IsValid { get; set; }

        public CustomerDTO User { get; set; } = null!;
    }
}
