using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class Message
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }

    public int SenderId { get; set; }

    public string Content { get; set; } = null!;

    public string MessageType { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public bool? IsRead { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    public virtual ChatRoom Room { get; set; } = null!;

    public virtual Customer Sender { get; set; } = null!;
}
