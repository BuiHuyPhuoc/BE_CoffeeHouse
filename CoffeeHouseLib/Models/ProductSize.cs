using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class ProductSize
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string Size { get; set; } = null!;

    public decimal Price { get; set; }

    public bool IsValid { get; set; }

    public virtual Product Product { get; set; } = null!;
}
