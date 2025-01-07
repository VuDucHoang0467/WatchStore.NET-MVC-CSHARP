using System;
using System.Collections.Generic;

namespace WebDongHo.Models;

public partial class Customization
{
    public int CustomizationId { get; set; }

    public int? UserId { get; set; }

    public int? WatchId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<CustomizationDetail> CustomizationDetails { get; set; } = new List<CustomizationDetail>();

    public virtual User? User { get; set; }

    public virtual Product? Watch { get; set; }
}
