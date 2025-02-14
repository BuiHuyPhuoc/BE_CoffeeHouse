using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class Address
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string AddressNumber { get; set; } = null!;

    public bool IsDefault { get; set; }

    public string Ward { get; set; } = null!;

    public string District { get; set; } = null!;

    public string Province { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public bool IsValid { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
