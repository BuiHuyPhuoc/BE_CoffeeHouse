using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class Voucher
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string DiscountType { get; set; } = null!;

    public decimal DiscountValue { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int? UsageLimit { get; set; }

    public int LitmitPerUser { get; set; }

    public bool IsActive { get; set; }

    public decimal? MinOrderValue { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
