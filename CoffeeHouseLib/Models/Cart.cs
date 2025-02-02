using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class Cart
{
    public int CustomerId { get; set; }

    public int ProductSizeId { get; set; }

    public int Quantity { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int Id { get; set; }

    public virtual ICollection<CartDetail> CartDetails { get; set; } = new List<CartDetail>();

    public virtual Customer Customer { get; set; } = null!;

    public virtual ProductSize ProductSize { get; set; } = null!;
}
