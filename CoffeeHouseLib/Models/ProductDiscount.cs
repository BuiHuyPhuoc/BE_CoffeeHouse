using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class ProductDiscount
{
    public int Id { get; set; }

    public int DiscountId { get; set; }

    public int ProductId { get; set; }

    public bool IsActive { get; set; }

    public virtual Discount Discount { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
