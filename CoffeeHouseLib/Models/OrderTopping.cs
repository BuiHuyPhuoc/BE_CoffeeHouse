using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class OrderTopping
{
    public int OrderDetailId { get; set; }

    public int ToppingId { get; set; }

    public int Quantity { get; set; }

    public virtual OrderDetail OrderDetail { get; set; } = null!;

    public virtual Topping Topping { get; set; } = null!;
}
