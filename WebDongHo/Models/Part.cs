using System;
using System.Collections.Generic;

namespace WebDongHo.Models;

public partial class Part
{
    public int PartId { get; set; }

    public string Name { get; set; } = null!;

    public string Type { get; set; } = null!;

    public virtual ICollection<PartOption> PartOptions { get; set; } = new List<PartOption>();
}
