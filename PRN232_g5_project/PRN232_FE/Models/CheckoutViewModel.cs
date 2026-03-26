using System.Collections.Generic;

namespace PRN232_FE.Models
{
    public class AddressViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }

    public class CheckoutViewModel
    {
        public ProductViewModel Product { get; set; } = new ProductViewModel();
        public List<AddressViewModel> Addresses { get; set; } = new List<AddressViewModel>();
        public int SelectedAddressId { get; set; }
        public string PaymentMethod { get; set; } = "credit_card";
        public decimal UserBalance { get; set; }
    }
}
