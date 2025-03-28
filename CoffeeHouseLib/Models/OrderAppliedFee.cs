using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class OrderAppliedFee
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int FeeId { get; set; }

    public decimal AppliedAmount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual OrderFee Fee { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
