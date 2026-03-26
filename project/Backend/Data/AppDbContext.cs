using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // =========================
    // DbSet
    // =========================
    public DbSet<FeedbackDetail> FeedbackDetails { get; set; }
    public DbSet<FeedbackReply> FeedbackReplies { get; set; }
    public DbSet<SellerToBuyerFeedback> SellerToBuyerFeedbacks { get; set; }
    public DbSet<Order> Orders { get; set; }

    // =========================
    // CONFIG
    // =========================
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ===== MAP TABLE (QUAN TRỌNG NHẤT) =====
        modelBuilder.Entity<FeedbackDetail>().ToTable("FeedbackDetail");
        modelBuilder.Entity<FeedbackReply>().ToTable("FeedbackReply");
        modelBuilder.Entity<SellerToBuyerFeedback>().ToTable("SellerToBuyerFeedback");
        modelBuilder.Entity<Order>().ToTable("Order"); // tránh lỗi keyword SQL

        // ===== UNIQUE CONSTRAINT =====
        modelBuilder.Entity<FeedbackDetail>()
            .HasIndex(f => f.OrderId)
            .IsUnique();

        modelBuilder.Entity<FeedbackReply>()
            .HasIndex(r => r.FeedbackDetailId)
            .IsUnique();

        modelBuilder.Entity<SellerToBuyerFeedback>()
            .HasIndex(s => s.OrderId)
            .IsUnique();

        // ===== OPTIONAL: COLUMN CONFIG =====
        modelBuilder.Entity<FeedbackDetail>()
            .Property(f => f.Comment)
            .HasMaxLength(500);

        modelBuilder.Entity<FeedbackReply>()
            .Property(r => r.ReplyContent)
            .HasMaxLength(500);

        modelBuilder.Entity<SellerToBuyerFeedback>()
            .Property(s => s.Message)
            .HasColumnType("nvarchar(max)");

        base.OnModelCreating(modelBuilder);
    }
}