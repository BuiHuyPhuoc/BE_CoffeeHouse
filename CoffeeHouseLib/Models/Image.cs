using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class Image
{
    public int Id { get; set; }

    public string ImageName { get; set; } = null!;

    public string ImageType { get; set; } = null!;

    public int ImageClassId { get; set; }

    public string? FirebaseImage { get; set; }

    public virtual ImageClass ImageClass { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<Product> ProductsNavigation { get; set; } = new List<Product>();
}
