using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class Order
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public DateTime OrderDate { get; set; }

    public string Status { get; set; } = null!;

    public int? VoucherId { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<OrderLog> OrderLogs { get; set; } = new List<OrderLog>();

    public virtual Voucher? Voucher { get; set; }
}
