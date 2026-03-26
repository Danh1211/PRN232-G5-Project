namespace PRN232_FE.Models
{
    public class StoreCreateRequest
    {
        public string StoreName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BannerImageUrl { get; set; } = string.Empty;
    }
}
