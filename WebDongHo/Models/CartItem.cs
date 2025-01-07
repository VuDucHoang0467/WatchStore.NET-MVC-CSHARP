using System;
using System.Collections.Generic;

namespace WebDongHo.Models;

public partial class CartItem
{
    public int ProductId { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public virtual Product Product { get; set; }
}
