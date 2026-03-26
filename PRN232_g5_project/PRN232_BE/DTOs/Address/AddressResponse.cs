namespace PRN232_BE.DTOs.Address
{
    public class AddressResponse
    {
        public int UserId { get; set; }
        public string Phone { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public bool IsDefault { get; set; }
    }
}
