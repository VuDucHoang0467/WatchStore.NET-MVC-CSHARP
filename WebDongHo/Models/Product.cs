using System;
using System.Collections.Generic;

namespace WebDongHo.Models;

public partial class Product
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public int StockQuantity { get; set; }

    public int? CategoryId { get; set; }

    public int? BrandId { get; set; }

    public string? Image { get; set; }

    public int ProductId { get; set; }

    public virtual Brand? Brand { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<Customization> Customizations { get; set; } = new List<Customization>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
}
