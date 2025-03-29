namespace CoffeeHouseAPI.DTOs.Chat
{
    public class SendMessageDTO
    {
        public string Message { get; set; } = "";
        public Guid RoomId { get; set; }
        public int SenderId { get; set; }
        public string MessageType { get; set; } = null!;
        public bool IsRead { get; set; } = true;
    }
}
