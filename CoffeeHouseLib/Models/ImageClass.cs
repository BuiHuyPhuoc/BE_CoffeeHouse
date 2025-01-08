using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class ImageClass
{
    public int Id { get; set; }

    public string ImageClassCode { get; set; } = null!;

    public string ImageClassName { get; set; } = null!;

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();
}
