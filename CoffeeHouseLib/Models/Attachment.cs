using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class Attachment
{
    public Guid Id { get; set; }

    public Guid MessageId { get; set; }

    public string FilePath { get; set; } = null!;

    public string FileType { get; set; } = null!;

    public string FileContent { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Message Message { get; set; } = null!;
}
