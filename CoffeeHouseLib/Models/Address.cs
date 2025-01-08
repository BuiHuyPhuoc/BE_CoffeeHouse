using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class Address
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string Address1 { get; set; } = null!;

    public bool IsDefault { get; set; }

    public virtual Customer Customer { get; set; } = null!;
}
