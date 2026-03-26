namespace PRN232_FE.Models
{
    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ProductViewModel
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

    public class HomeViewModel
    {
        public List<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
        public List<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
    }
}
