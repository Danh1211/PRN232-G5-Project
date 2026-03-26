using System.ComponentModel.DataAnnotations;

namespace PRN232_BE.DTOs.Product
{
    public class ProductCreateRequest
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, MinimumLength = 1)]
        public string Title { get; set; }
        public string Description { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "CategoryId is required.")]
        public int CategoryId { get; set; }
        
        public string? ThumbnailUrl { get; set; }
    }
}
