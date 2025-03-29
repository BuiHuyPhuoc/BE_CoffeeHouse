using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class UserConnection
{
    public Guid Id { get; set; }

    public int UserId { get; set; }

    public string ConnectionId { get; set; } = null!;

    public DateTime? ConnectedAt { get; set; }

    public bool? IsOnline { get; set; }

    public virtual Customer User { get; set; } = null!;
}
