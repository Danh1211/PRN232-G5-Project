using System;
using System.Collections.Generic;

namespace PRN232_BE.Models;

public partial class Room
{
    public int Id { get; set; }

    public string Type { get; set; } = null!;

    public int SellerId { get; set; }

    public int BuyerId { get; set; }

    public int? OrderId { get; set; }

    public int? DisputeId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User Buyer { get; set; } = null!;

    public virtual Dispute? Dispute { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual OrderTable? Order { get; set; }

    public virtual User Seller { get; set; } = null!;
}
