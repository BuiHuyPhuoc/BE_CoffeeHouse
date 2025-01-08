using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class OrderDetail
{
    public int Id { get; set; }

    public int ProductSizeId { get; set; }

    public int OrderId { get; set; }

    public string? Note { get; set; }

    public int? DiscountId { get; set; }

    public virtual Discount? Discount { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual OrderTopping? OrderTopping { get; set; }
}
