using System;
using System.Collections.Generic;

namespace WebDongHo.Models;

public partial class CustomizationDetail
{
    public int CustomizationDetailId { get; set; }

    public int? CustomizationId { get; set; }

    public int? PartOptionId { get; set; }

    public int? PartId { get; set; }

    public virtual Customization? Customization { get; set; }

    public virtual PartOption? PartOption { get; set; }
}
