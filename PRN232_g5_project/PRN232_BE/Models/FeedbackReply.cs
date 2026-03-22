using System;
using System.Collections.Generic;

namespace PRN232_BE.Models;

public partial class FeedbackReply
{
    public int Id { get; set; }

    public int FeedbackDetailId { get; set; }

    public int SellerId { get; set; }

    public string ReplyContent { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual FeedbackDetail FeedbackDetail { get; set; } = null!;

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual User Seller { get; set; } = null!;
}
