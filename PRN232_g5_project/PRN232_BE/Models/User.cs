using System;
using System.Collections.Generic;

namespace PRN232_BE.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public decimal Balance { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual ICollection<Dispute> DisputeRaisedByNavigations { get; set; } = new List<Dispute>();

    public virtual ICollection<Dispute> DisputeSellers { get; set; } = new List<Dispute>();

    public virtual ICollection<FeedbackDetail> FeedbackDetails { get; set; } = new List<FeedbackDetail>();

    public virtual ICollection<FeedbackReply> FeedbackReplies { get; set; } = new List<FeedbackReply>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<OrderTable> OrderTableBuyers { get; set; } = new List<OrderTable>();

    public virtual ICollection<OrderTable> OrderTableSellers { get; set; } = new List<OrderTable>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<ReturnRequest> ReturnRequests { get; set; } = new List<ReturnRequest>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Room> RoomBuyers { get; set; } = new List<Room>();

    public virtual ICollection<Room> RoomSellers { get; set; } = new List<Room>();

    public virtual ICollection<SellerToBuyerFeedback> SellerToBuyerFeedbackBuyers { get; set; } = new List<SellerToBuyerFeedback>();

    public virtual ICollection<SellerToBuyerFeedback> SellerToBuyerFeedbackSellers { get; set; } = new List<SellerToBuyerFeedback>();

    public virtual ICollection<Store> Stores { get; set; } = new List<Store>();
}
