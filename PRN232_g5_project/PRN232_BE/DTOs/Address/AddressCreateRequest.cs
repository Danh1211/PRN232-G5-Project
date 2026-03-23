using System.ComponentModel.DataAnnotations;

namespace PRN232_BE.DTOs.Address
{
    public class AddressCreateRequest
    {

        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Phone number must be between 1 and 20 characters.")]
        public string Phone { get; set; } = null!;

        [Required(ErrorMessage = "Street is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Street must be between 1 and 200 characters.")]
        public string Street { get; set; } = null!;

        [Required(ErrorMessage = "City is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "City must be between 1 and 50 characters.")]
        public string City { get; set; } = null!;

        [Required(ErrorMessage = "State is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "State must be between 1 and 50 characters.")]
        public string Country { get; set; } = null!;

        [Required(ErrorMessage = "Is default is required.")]
        public bool IsDefault { get; set; }
    }
}
