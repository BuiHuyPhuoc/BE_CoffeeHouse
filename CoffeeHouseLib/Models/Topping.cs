﻿using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class Topping
{
    public int Id { get; set; }

    public string ToppingName { get; set; } = null!;

    public decimal ToppingPrice { get; set; }

    public bool IsValid { get; set; }

    public virtual ICollection<CartDetail> CartDetails { get; set; } = new List<CartDetail>();

    public virtual ICollection<OrderTopping> OrderToppings { get; set; } = new List<OrderTopping>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
