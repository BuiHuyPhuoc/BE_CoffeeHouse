using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class Customer
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public DateTime? DateOfBirth { get; set; }

    public string? Phone { get; set; } = null!;

    public bool IdRole { get; set; }

    public string Email { get; set; } = null!;

    public bool IsValid { get; set; }

    public int? AvatarImage { get; set; }

    public virtual Account? Account { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual Image? AvatarImageNavigation { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<RoomUser> RoomUsers { get; set; } = new List<RoomUser>();

    public virtual ICollection<UserConnection> UserConnections { get; set; } = new List<UserConnection>();
}
