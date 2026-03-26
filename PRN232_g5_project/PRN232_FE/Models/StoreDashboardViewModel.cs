namespace PRN232_FE.Models
{
    public class StoreDashboardViewModel
    {
        public int SellerId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BannerImageUrl { get; set; } = string.Empty;
        public int ReputationScore { get; set; }
        
        public List<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
    }
}
