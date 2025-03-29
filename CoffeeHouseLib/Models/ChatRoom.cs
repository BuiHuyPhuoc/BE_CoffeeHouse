using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class ChatRoom
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string RoomType { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public bool IsValid { get; set; }

    public DateTime? LastMessageAt { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<RoomUser> RoomUsers { get; set; } = new List<RoomUser>();
}
