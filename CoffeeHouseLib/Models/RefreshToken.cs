using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class RefreshToken
{
    public string RefreshToken1 { get; set; } = null!;

    public DateTime? Expire { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Revoke { get; set; }

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}
