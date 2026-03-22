using System;
using System.Collections.Generic;

namespace PRN232_BE.Models;

public partial class SellerToBuyerFeedback
{
    public int Id { get; set; }

    public int SellerId { get; set; }

    public int BuyerId { get; set; }

    public int ProductId { get; set; }

    public string Message { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual User Buyer { get; set; } = null!;

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual Product Product { get; set; } = null!;

    public virtual User Seller { get; set; } = null!;
}
