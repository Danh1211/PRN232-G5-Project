using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PRN232_BE.Models;

namespace PRN232_BE;

public partial class CloneEbayDb1Context : DbContext
{
    public CloneEbayDb1Context()
    {
    }

    public CloneEbayDb1Context(DbContextOptions<CloneEbayDb1Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Dispute> Disputes { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<FeedbackDetail> FeedbackDetails { get; set; }

    public virtual DbSet<FeedbackReply> FeedbackReplies { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<OrderTable> OrderTables { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ReturnRequest> ReturnRequests { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<SellerToBuyerFeedback> SellerToBuyerFeedbacks { get; set; }

    public virtual DbSet<ShippingInfo> ShippingInfos { get; set; }

    public virtual DbSet<Store> Stores { get; set; }

    public virtual DbSet<User> Users { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=(local);Database=CloneEbayDB1;UID=sa;PWD=123;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Address__3213E83F9611AFD6");

            entity.ToTable("Address");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .HasColumnName("city");
            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .HasColumnName("country");
            entity.Property(e => e.IsDefault).HasColumnName("isDefault");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Street)
                .HasMaxLength(100)
                .HasColumnName("street");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.User).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Address__userId__48CFD27E");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3213E83F079EBE5A");

            entity.ToTable("Category");

            entity.HasIndex(e => e.Name, "UQ__Category__72E12F1BF906F1EE").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Dispute>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Dispute__3213E83F2209DEC0");

            entity.ToTable("Dispute");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AdminJoin).HasColumnName("adminJoin");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.OrderId).HasColumnName("orderId");
            entity.Property(e => e.RaisedBy).HasColumnName("raisedBy");
            entity.Property(e => e.Resolution).HasColumnName("resolution");
            entity.Property(e => e.ResolutionType)
                .HasMaxLength(50)
                .HasColumnName("resolutionType");
            entity.Property(e => e.ResolvedAt)
                .HasColumnType("datetime")
                .HasColumnName("resolvedAt");
            entity.Property(e => e.SellerId).HasColumnName("sellerId");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.WinnerIsBuyer).HasColumnName("winnerIsBuyer");

            entity.HasOne(d => d.Order).WithMany(p => p.Disputes)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Dispute__orderId__5AEE82B9");

            entity.HasOne(d => d.RaisedByNavigation).WithMany(p => p.DisputeRaisedByNavigations)
                .HasForeignKey(d => d.RaisedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Dispute__raisedB__5BE2A6F2");

            entity.HasOne(d => d.Seller).WithMany(p => p.DisputeSellers)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Dispute__sellerI__5CD6CB2B");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Feedback__3213E83FD4E90198");

            entity.ToTable("Feedback");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AverageRating)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("averageRating");
            entity.Property(e => e.NegativeRate)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("negativeRate");
            entity.Property(e => e.PositiveRate)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("positiveRate");
            entity.Property(e => e.SellerId).HasColumnName("sellerId");

            entity.HasOne(d => d.Seller).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedback__seller__619B8048");
        });

        modelBuilder.Entity<FeedbackDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Feedback__3213E83F80782166");

            entity.ToTable("FeedbackDetail");

            entity.HasIndex(e => e.OrderId, "UQ__Feedback__0809335C54F94E1D").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BuyerId).HasColumnName("buyerId");
            entity.Property(e => e.Comment)
                .HasMaxLength(500)
                .HasColumnName("comment");
            entity.Property(e => e.Communication).HasColumnName("communication");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.FeedbackId).HasColumnName("feedbackId");
            entity.Property(e => e.ItemAsDescribed).HasColumnName("itemAsDescribed");
            entity.Property(e => e.OrderId).HasColumnName("orderId");
            entity.Property(e => e.ShippingCost).HasColumnName("shippingCost");
            entity.Property(e => e.ShippingTime).HasColumnName("shippingTime");
            entity.Property(e => e.Type).HasColumnName("type");

            entity.HasOne(d => d.Buyer).WithMany(p => p.FeedbackDetails)
                .HasForeignKey(d => d.BuyerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FeedbackD__buyer__66603565");

            entity.HasOne(d => d.Feedback).WithMany(p => p.FeedbackDetails)
                .HasForeignKey(d => d.FeedbackId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FeedbackD__feedb__656C112C");

            entity.HasOne(d => d.Order).WithOne(p => p.FeedbackDetail)
                .HasForeignKey<FeedbackDetail>(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FeedbackD__order__6754599E");
        });

        modelBuilder.Entity<FeedbackReply>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Feedback__3213E83F095FBF7B");

            entity.ToTable("FeedbackReply");

            entity.HasIndex(e => e.FeedbackDetailId, "UQ__Feedback__465B88706B46AE41").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.FeedbackDetailId).HasColumnName("feedbackDetailId");
            entity.Property(e => e.ReplyContent)
                .HasMaxLength(500)
                .HasColumnName("replyContent");
            entity.Property(e => e.SellerId).HasColumnName("sellerId");

            entity.HasOne(d => d.FeedbackDetail).WithOne(p => p.FeedbackReply)
                .HasForeignKey<FeedbackReply>(d => d.FeedbackDetailId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FeedbackR__feedb__6C190EBB");

            entity.HasOne(d => d.Seller).WithMany(p => p.FeedbackReplies)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FeedbackR__selle__6D0D32F4");
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Image__3213E83F79F7B61D");

            entity.ToTable("Image");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DisputeId).HasColumnName("disputeId");
            entity.Property(e => e.FeedbackDetailId).HasColumnName("feedbackDetailId");
            entity.Property(e => e.FeedbackReplyId).HasColumnName("feedbackReplyId");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(1000)
                .HasColumnName("imageUrl");
            entity.Property(e => e.MessageId).HasColumnName("messageId");
            entity.Property(e => e.ReviewId).HasColumnName("reviewId");
            entity.Property(e => e.SellerToBuyerFeedbackId).HasColumnName("sellerToBuyerFeedbackId");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("uploadedAt");

            entity.HasOne(d => d.Dispute).WithMany(p => p.Images)
                .HasForeignKey(d => d.DisputeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Image__disputeId__0D7A0286");

            entity.HasOne(d => d.FeedbackDetail).WithMany(p => p.Images)
                .HasForeignKey(d => d.FeedbackDetailId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Image__feedbackD__0C85DE4D");

            entity.HasOne(d => d.FeedbackReply).WithMany(p => p.Images)
                .HasForeignKey(d => d.FeedbackReplyId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Image__feedbackR__0F624AF8");

            entity.HasOne(d => d.Message).WithMany(p => p.Images)
                .HasForeignKey(d => d.MessageId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Image__messageId__0A9D95DB");

            entity.HasOne(d => d.Review).WithMany(p => p.Images)
                .HasForeignKey(d => d.ReviewId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Image__reviewId__0B91BA14");

            entity.HasOne(d => d.SellerToBuyerFeedback).WithMany(p => p.Images)
                .HasForeignKey(d => d.SellerToBuyerFeedbackId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Image__sellerToB__0E6E26BF");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Message__3213E83F64BEAB72");

            entity.ToTable("Message");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.Date)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.IsRead).HasColumnName("isRead");
            entity.Property(e => e.RoomId).HasColumnName("roomId");
            entity.Property(e => e.SenderId).HasColumnName("senderId");

            entity.HasOne(d => d.Room).WithMany(p => p.Messages)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Message__roomId__03F0984C");

            entity.HasOne(d => d.Sender).WithMany(p => p.Messages)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Message__senderI__04E4BC85");
        });

        modelBuilder.Entity<OrderTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderTab__3213E83F63D0B161");

            entity.ToTable("OrderTable");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddressId).HasColumnName("addressId");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.BuyerId).HasColumnName("buyerId");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("orderDate");
            entity.Property(e => e.ProductId).HasColumnName("productId");
            entity.Property(e => e.SellerId).HasColumnName("sellerId");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");

            entity.HasOne(d => d.Address).WithMany(p => p.OrderTables)
                .HasForeignKey(d => d.AddressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderTabl__addre__4E88ABD4");

            entity.HasOne(d => d.Buyer).WithMany(p => p.OrderTableBuyers)
                .HasForeignKey(d => d.BuyerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderTabl__buyer__4CA06362");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderTables)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderTabl__produ__4F7CD00D");

            entity.HasOne(d => d.Seller).WithMany(p => p.OrderTableSellers)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderTabl__selle__4D94879B");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payment__3213E83F592F0E23");

            entity.ToTable("Payment");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.BuyerId).HasColumnName("buyerId");
            entity.Property(e => e.Method)
                .HasMaxLength(50)
                .HasColumnName("method");
            entity.Property(e => e.OrderId).HasColumnName("orderId");
            entity.Property(e => e.PaidAt)
                .HasColumnType("datetime")
                .HasColumnName("paidAt");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");

            entity.HasOne(d => d.Buyer).WithMany(p => p.Payments)
                .HasForeignKey(d => d.BuyerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payment__buyerId__5441852A");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payment__orderId__534D60F1");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Product__3213E83FE4E2E5AE");

            entity.ToTable("Product");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AuctionEndTime)
                .HasColumnType("datetime")
                .HasColumnName("auctionEndTime");
            entity.Property(e => e.CategoryId).HasColumnName("categoryId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsAuction).HasColumnName("isAuction");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.SellerId).HasColumnName("sellerId");
            entity.Property(e => e.StoreId).HasColumnName("storeId");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Product__categor__4222D4EF");

            entity.HasOne(d => d.Seller).WithMany(p => p.Products)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Product__sellerI__4316F928");

            entity.HasOne(d => d.Store).WithMany(p => p.Products)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Product__storeId__440B1D61");
        });

        modelBuilder.Entity<ReturnRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ReturnRe__3213E83FD8147FD2");

            entity.ToTable("ReturnRequest");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.OrderId).HasColumnName("orderId");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.Order).WithMany(p => p.ReturnRequests)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ReturnReq__order__123EB7A3");

            entity.HasOne(d => d.User).WithMany(p => p.ReturnRequests)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ReturnReq__userI__1332DBDC");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Review__3213E83F7D736CC9");

            entity.ToTable("Review");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.OrderId).HasColumnName("orderId");
            entity.Property(e => e.ProductId).HasColumnName("productId");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.ReviewerId).HasColumnName("reviewerId");

            entity.HasOne(d => d.Order).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__Review__orderId__76969D2E");

            entity.HasOne(d => d.Product).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Review__productI__778AC167");

            entity.HasOne(d => d.Reviewer).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.ReviewerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Review__reviewer__787EE5A0");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Room__3213E83FDBB349E9");

            entity.ToTable("Room");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BuyerId).HasColumnName("buyerId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.DisputeId).HasColumnName("disputeId");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("isActive");
            entity.Property(e => e.OrderId).HasColumnName("orderId");
            entity.Property(e => e.SellerId).HasColumnName("sellerId");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .HasColumnName("type");

            entity.HasOne(d => d.Buyer).WithMany(p => p.RoomBuyers)
                .HasForeignKey(d => d.BuyerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Room__buyerId__7D439ABD");

            entity.HasOne(d => d.Dispute).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.DisputeId)
                .HasConstraintName("FK__Room__disputeId__7F2BE32F");

            entity.HasOne(d => d.Order).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__Room__orderId__7E37BEF6");

            entity.HasOne(d => d.Seller).WithMany(p => p.RoomSellers)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Room__sellerId__7C4F7684");
        });

        modelBuilder.Entity<SellerToBuyerFeedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SellerTo__3213E83F15D920EB");

            entity.ToTable("SellerToBuyerFeedback");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BuyerId).HasColumnName("buyerId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.ProductId).HasColumnName("productId");
            entity.Property(e => e.SellerId).HasColumnName("sellerId");

            entity.HasOne(d => d.Buyer).WithMany(p => p.SellerToBuyerFeedbackBuyers)
                .HasForeignKey(d => d.BuyerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SellerToB__buyer__71D1E811");

            entity.HasOne(d => d.Product).WithMany(p => p.SellerToBuyerFeedbacks)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SellerToB__produ__72C60C4A");

            entity.HasOne(d => d.Seller).WithMany(p => p.SellerToBuyerFeedbackSellers)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SellerToB__selle__70DDC3D8");
        });

        modelBuilder.Entity<ShippingInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Shipping__3213E83F7A19A1BF");

            entity.ToTable("ShippingInfo");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Carrier)
                .HasMaxLength(100)
                .HasColumnName("carrier");
            entity.Property(e => e.EstimatedArrival)
                .HasColumnType("datetime")
                .HasColumnName("estimatedArrival");
            entity.Property(e => e.HasSignature).HasColumnName("hasSignature");
            entity.Property(e => e.OrderId).HasColumnName("orderId");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.TrackingNumber)
                .HasMaxLength(100)
                .HasColumnName("trackingNumber");

            entity.HasOne(d => d.Order).WithMany(p => p.ShippingInfos)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShippingI__order__571DF1D5");
        });

        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Store__3213E83F9C994A28");

            entity.ToTable("Store");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BannerImageUrl).HasColumnName("bannerImageURL");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.SellerId).HasColumnName("sellerId");
            entity.Property(e => e.StoreName)
                .HasMaxLength(100)
                .HasColumnName("storeName");

            entity.HasOne(d => d.Seller).WithMany(p => p.Stores)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Store__sellerId__3C69FB99");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3213E83FD3B3E65B");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ__User__AB6E6164FDB861F3").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AvatarUrl).HasColumnName("avatarURL");
            entity.Property(e => e.Balance)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("balance");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasColumnName("role");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
