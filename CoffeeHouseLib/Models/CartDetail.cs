using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class CartDetail
{
    public int? CartId { get; set; }

    public int? ToppingId { get; set; }

    public int? Quantity { get; set; }

    public int Id { get; set; }

    public virtual Cart? Cart { get; set; }

    public virtual Topping? Topping { get; set; }
}
