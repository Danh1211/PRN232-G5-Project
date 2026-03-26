public class SellerToBuyerFeedback
{
    public int Id { get; set; }

    public int SellerId { get; set; }
    public int BuyerId { get; set; }
    public int ProductId { get; set; }
    public int OrderId { get; set; }

    public string? Message { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}