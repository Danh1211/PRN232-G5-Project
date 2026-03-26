namespace EbayClone.DTOs
{
    public class CreateFeedbackDto
    {
        public int BuyerId { get; set; }
        public int OrderId { get; set; }
        public byte Type { get; set; }
        public string Comment { get; set; }
    }

    public class UpdateFeedbackDto
    {
        public string Comment { get; set; }
    }

    public class CreateReplyDto
    {
        public int FeedbackDetailId { get; set; }
        public int SellerId { get; set; }
        public string Content { get; set; }
    }

    public class SellerFeedbackDto
    {
        public int SellerId { get; set; }
        public int OrderId { get; set; }
        public string Message { get; set; }
    }
}