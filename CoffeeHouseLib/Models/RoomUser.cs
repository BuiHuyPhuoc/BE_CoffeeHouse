using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class RoomUser
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }

    public int UserId { get; set; }

    public DateTime? JoinedAt { get; set; }

    public string? Role { get; set; }

    public virtual ChatRoom Room { get; set; } = null!;

    public virtual Customer User { get; set; } = null!;
}
