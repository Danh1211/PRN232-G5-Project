using System;
using System.Collections.Generic;

namespace PRN232_BE.Models;

public partial class Product
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int CategoryId { get; set; }

    public int SellerId { get; set; }

    public int StoreId { get; set; }

    public bool IsAuction { get; set; }

    public DateTime? AuctionEndTime { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? ThumbnailUrl { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<OrderTable> OrderTables { get; set; } = new List<OrderTable>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual User Seller { get; set; } = null!;

    public virtual ICollection<SellerToBuyerFeedback> SellerToBuyerFeedbacks { get; set; } = new List<SellerToBuyerFeedback>();

    public virtual Store Store { get; set; } = null!;
}
