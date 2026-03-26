using System;
using System.Collections.Generic;

namespace PRN232_BE.Models;

public partial class Dispute
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int RaisedBy { get; set; }

    public int SellerId { get; set; }

    public string Description { get; set; } = null!;

    public string Status { get; set; } = null!;

    public bool AdminJoin { get; set; }

    public bool? WinnerIsBuyer { get; set; }

    public string? ResolutionType { get; set; }

    public string? Resolution { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual OrderTable Order { get; set; } = null!;

    public virtual User RaisedByNavigation { get; set; } = null!;

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

    public virtual User Seller { get; set; } = null!;
}
