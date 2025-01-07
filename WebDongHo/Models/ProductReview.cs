using System;
using System.Collections.Generic;

namespace WebDongHo.Models;

public partial class ProductReview
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int UserId { get; set; }

    public string Email { get; set; } = null!;

    public int Rating { get; set; }

    public string? ReviewText { get; set; }

    public DateTime ReviewDate { get; set; }

    public string? AdminResponse { get; set; }

    public DateTime? AdminResponseDate { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
