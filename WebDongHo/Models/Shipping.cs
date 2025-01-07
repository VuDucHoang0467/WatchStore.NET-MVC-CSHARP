using System;
using System.Collections.Generic;

namespace WebDongHo.Models;

public partial class Shipping
{
    public int ShippingId { get; set; }

    public int? OrderId { get; set; }

    public string ShippingMethod { get; set; } = null!;

    public string? TrackingNumber { get; set; }

    public DateTime ShipmentDate { get; set; }

    public DateTime? EstimatedDeliveryDate { get; set; }
}
