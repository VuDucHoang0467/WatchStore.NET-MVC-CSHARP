using System;
using System.Collections.Generic;

namespace WebDongHo.Models;

public partial class Order
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal? TotalPrice { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Phone { get; set; }

    public string? ShippingAddress { get; set; }

    public string? Notes { get; set; }

    public string? Email { get; set; }

    public int? Status { get; set; }

    public DateTime? ReceiveTime { get; set; }

    public string? CouponCode { get; set; }

    public decimal? DiscountPercentage { get; set; }

    public string? PaymentMethod { get; set; }

    public string? PaymentStatus { get; set; }

    public DateTime? PaymentTime { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual User? User { get; set; }
}
