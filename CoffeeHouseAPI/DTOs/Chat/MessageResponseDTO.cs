namespace CoffeeHouseAPI.DTOs.Chat
{
    public class MessageResponseDTO
    {
        public string Message { get; set; } = "";
        public Guid RoomId { get; set; }
        public int SenderId { get; set; }
        public string MessageType { get; set; } = null!;
        public bool IsRead { get; set; } = true;
        public DateTime? CreatedAt { get; set; }
    }
}
