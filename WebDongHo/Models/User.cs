using System;
using System.Collections.Generic;

namespace WebDongHo.Models;

public partial class User
{
    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int? RoleId { get; set; }

    public int UserId { get; set; }

    public string? ResetPasswordToken { get; set; }

    public DateTime? ResetPasswordExpiration { get; set; }

    public bool? EmailConfirmed { get; set; }

    public string? EmailConfirmationToken { get; set; }

    public virtual ICollection<Customization> Customizations { get; set; } = new List<Customization>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
}
