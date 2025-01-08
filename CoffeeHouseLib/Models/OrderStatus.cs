using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class OrderStatus
{
    public string StatusCode { get; set; } = null!;

    public string StatusName { get; set; } = null!;

    public virtual ICollection<OrderLog> OrderLogs { get; set; } = new List<OrderLog>();
}
