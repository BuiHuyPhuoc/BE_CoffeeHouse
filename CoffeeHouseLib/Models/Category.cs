using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class Category
{
    public int Id { get; set; }

    public string CategoryName { get; set; } = null!;

    public int? IdParent { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
