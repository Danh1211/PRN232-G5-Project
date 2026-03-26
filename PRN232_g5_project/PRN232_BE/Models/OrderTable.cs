using System;
using System.Collections.Generic;

namespace PRN232_BE.Models;

public partial class OrderTable
{
    public int Id { get; set; }

    public int BuyerId { get; set; }

    public int SellerId { get; set; }

    public int AddressId { get; set; }

    public int ProductId { get; set; }

    public DateTime OrderDate { get; set; }

    public int Amount { get; set; }

    public string Status { get; set; } = null!;

    public virtual Address Address { get; set; } = null!;

    public virtual User Buyer { get; set; } = null!;

    public virtual ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();

    public virtual FeedbackDetail? FeedbackDetail { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<ReturnRequest> ReturnRequests { get; set; } = new List<ReturnRequest>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

    public virtual User Seller { get; set; } = null!;

    public virtual ICollection<ShippingInfo> ShippingInfos { get; set; } = new List<ShippingInfo>();
}
