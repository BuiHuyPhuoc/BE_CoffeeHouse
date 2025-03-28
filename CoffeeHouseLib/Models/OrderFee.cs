using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class OrderFee
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string FeeType { get; set; } = null!;

    public decimal Amount { get; set; }

    public bool IsActive { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<OrderAppliedFee> OrderAppliedFees { get; set; } = new List<OrderAppliedFee>();
}
