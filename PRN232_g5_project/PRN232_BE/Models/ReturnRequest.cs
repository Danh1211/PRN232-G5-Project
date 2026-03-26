using System;
using System.Collections.Generic;

namespace PRN232_BE.Models;

public partial class ReturnRequest
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int UserId { get; set; }

    public string Reason { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual OrderTable Order { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
