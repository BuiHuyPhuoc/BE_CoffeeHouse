using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class OrderDetail
{
    public int Id { get; set; }

    public int ProductSizeId { get; set; }

    public int OrderId { get; set; }

    public string? Note { get; set; }

    public int Quantity { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ICollection<OrderTopping> OrderToppings { get; set; } = new List<OrderTopping>();

    public virtual ProductSize ProductSize { get; set; } = null!;
}
