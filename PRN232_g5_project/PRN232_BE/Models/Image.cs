using System;
using System.Collections.Generic;

namespace PRN232_BE.Models;

public partial class Image
{
    public int Id { get; set; }

    public string ImageUrl { get; set; } = null!;

    public DateTime UploadedAt { get; set; }

    public int? MessageId { get; set; }

    public int? ReviewId { get; set; }

    public int? FeedbackDetailId { get; set; }

    public int? DisputeId { get; set; }

    public int? SellerToBuyerFeedbackId { get; set; }

    public int? FeedbackReplyId { get; set; }

    public virtual Dispute? Dispute { get; set; }

    public virtual FeedbackDetail? FeedbackDetail { get; set; }

    public virtual FeedbackReply? FeedbackReply { get; set; }

    public virtual Message? Message { get; set; }

    public virtual Review? Review { get; set; }

    public virtual SellerToBuyerFeedback? SellerToBuyerFeedback { get; set; }
}
