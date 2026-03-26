using System.ComponentModel.DataAnnotations;

namespace PRN232_BE.DTOs.Store
{
    public class StoreCreateRequest
    {
        [Required(ErrorMessage = "Store name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Store name must be between 1 and 100 characters.")]
        public string StoreName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string BannerImageUrl { get; set; } = null!;
    }
}
