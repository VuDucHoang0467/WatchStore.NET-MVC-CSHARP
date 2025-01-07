using System;
using System.Collections.Generic;

namespace WebDongHo.Models;

public partial class DiscountCode
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public decimal DisPercentage { get; set; }

    public int? UsageLimit { get; set; }

    public DateTime? ExpiryDate { get; set; }
}
