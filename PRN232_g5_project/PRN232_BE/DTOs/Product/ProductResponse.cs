namespace PRN232_BE.DTOs.Product
{
    public class ProductResponse
    {
        public int Id { get; set; }
        public int SellerId { get; set; }
        public int StoreId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ThumbnailUrl { get; set; }
    }
}
