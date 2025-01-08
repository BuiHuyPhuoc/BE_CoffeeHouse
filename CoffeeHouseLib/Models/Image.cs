using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class Image
{
    public int Id { get; set; }

    public string ImageName { get; set; } = null!;

    public string ImageType { get; set; } = null!;

    public byte[] Content { get; set; } = null!;

    public int ImageClassId { get; set; }

    public virtual ImageClass ImageClass { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
