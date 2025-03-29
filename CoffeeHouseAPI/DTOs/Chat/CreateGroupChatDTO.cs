namespace CoffeeHouseAPI.DTOs.Chat
{
    public class CreateGroupChatDTO
    {
        public List<int> CustomerIds { get; set; } = new List<int>();
        public string RoomType { get; set; } = null!;
    }
}
