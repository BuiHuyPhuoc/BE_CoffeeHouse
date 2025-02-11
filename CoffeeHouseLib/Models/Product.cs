﻿using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class Product
{
    public int Id { get; set; }

    public string ProductName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int CategoryId { get; set; }

    public bool IsValid { get; set; }

    public int? ImageDefault { get; set; }

    public string? Description2 { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Material { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual Image? ImageDefaultNavigation { get; set; }

    public virtual ICollection<ProductDiscount> ProductDiscounts { get; set; } = new List<ProductDiscount>();

    public virtual ICollection<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual ICollection<Topping> Toppings { get; set; } = new List<Topping>();
}
