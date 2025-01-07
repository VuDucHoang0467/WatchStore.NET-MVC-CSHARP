using System;
using System.Collections.Generic;

namespace WebDongHo.Models;

public partial class Brand
{
    public string Name { get; set; } = null!;

    public int BrandId { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
