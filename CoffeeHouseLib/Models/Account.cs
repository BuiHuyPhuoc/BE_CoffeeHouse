using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class Account
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int CustomerId { get; set; }

    public string? VerifyToken { get; set; }

    public DateTime? VerifyTime { get; set; }

    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }

    public short LoginFailed { get; set; }

    public string? ResetPasswordToken { get; set; }

    public DateTime? ResetPasswordExpired { get; set; }

    public DateTime? BlockExpire { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual RefreshToken? RefreshTokenNavigation { get; set; }
}
