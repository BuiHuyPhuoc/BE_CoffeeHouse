using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class Aiconfig
{
    public int Id { get; set; }

    public string Promt { get; set; } = null!;

    public string? Key { get; set; }
}
