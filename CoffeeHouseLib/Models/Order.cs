using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public DateTime OrderDate { get; set; }

    public int? VoucherId { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<OrderLog> OrderLogs { get; set; } = new List<OrderLog>();

    public virtual Voucher? Voucher { get; set; }
}
