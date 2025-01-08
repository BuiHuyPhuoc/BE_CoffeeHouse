using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class OrderLog
{
    public string StatusCode { get; set; } = null!;

    public int OrderId { get; set; }

    public DateOnly TimeLog { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual OrderStatus StatusCodeNavigation { get; set; } = null!;
}
