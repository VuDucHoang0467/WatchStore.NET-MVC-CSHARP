using System;
using System.Collections.Generic;

namespace WebDongHo.Models;

public partial class PartOption
{
    public int PartOptionId { get; set; }

    public int? PartId { get; set; }

    public string? OptionName { get; set; }

    public string? OptionValue { get; set; }

    public virtual ICollection<CustomizationDetail> CustomizationDetails { get; set; } = new List<CustomizationDetail>();

    public virtual Part? Part { get; set; }
}
