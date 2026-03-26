using System;
using System.Collections.Generic;

namespace PRN232_BE.Models;

public partial class Payment
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int BuyerId { get; set; }

    public decimal Amount { get; set; }

    public string Method { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? PaidAt { get; set; }

    public virtual User Buyer { get; set; } = null!;

    public virtual OrderTable Order { get; set; } = null!;
}
