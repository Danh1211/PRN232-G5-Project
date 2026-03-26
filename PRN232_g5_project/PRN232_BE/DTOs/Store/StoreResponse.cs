namespace PRN232_BE.DTOs.Store
{
    public class StoreResponse
    {
        public int SellerId { get; set; }
        public string StoreName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string BannerImageUrl { get; set; } = null!;
    }
}
