using System;
using System.Collections.Generic;

namespace PRN232_BE.Models;

public partial class Feedback
{
    public int Id { get; set; }

    public int SellerId { get; set; }

    public decimal? AverageRating { get; set; }

    public decimal? PositiveRate { get; set; }

    public decimal? NegativeRate { get; set; }

    public virtual ICollection<FeedbackDetail> FeedbackDetails { get; set; } = new List<FeedbackDetail>();

    public virtual User Seller { get; set; } = null!;
}
