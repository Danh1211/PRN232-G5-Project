public class FeedbackDetail
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

    public bool IsUpdated { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}