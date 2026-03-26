public class Feedback
{
    public int Id { get; set; }
    public int SellerId { get; set; }

    public decimal AverageRating { get; set; }
    public decimal PositiveRate { get; set; }
    public decimal NegativeRate { get; set; }

    public ICollection<FeedbackDetail> FeedbackDetails { get; set; }
}