public class FeedbackReply
{
    public int Id { get; set; }

    public int FeedbackDetailId { get; set; }
    public int SellerId { get; set; }

    public string? ReplyContent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}