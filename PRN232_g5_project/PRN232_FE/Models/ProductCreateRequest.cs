namespace PRN232_FE.Models
{
    public class ProductCreateRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public string ThumbnailUrl { get; set; } = string.Empty;
    }
}
