using System;
using System.Collections.Generic;

namespace WebDongHo.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int? OrderId { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }
}
