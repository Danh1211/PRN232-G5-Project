using System;
using System.Collections.Generic;

namespace PRN232_BE.Models;

public partial class FeedbackDetail
{
    public int Id { get; set; }

    public int FeedbackId { get; set; }

    public int BuyerId { get; set; }

    public int OrderId { get; set; }

    public byte Type { get; set; }

    public byte? ItemAsDescribed { get; set; }

    public byte? Communication { get; set; }

    public byte? ShippingTime { get; set; }

    public byte? ShippingCost { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User Buyer { get; set; } = null!;

    public virtual Feedback Feedback { get; set; } = null!;

    public virtual FeedbackReply? FeedbackReply { get; set; }

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual OrderTable Order { get; set; } = null!;
}
