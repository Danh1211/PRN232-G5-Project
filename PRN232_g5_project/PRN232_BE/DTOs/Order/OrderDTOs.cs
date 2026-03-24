using PRN232_BE.Models;


public class OrderResponse
{
    public int Id { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }

    // Thêm cho detail
    public string? SellerUsername { get; set; }
    public string? BuyerUsername { get; set; }
    public AddressDto? Address { get; set; }
    public PaymentDto? Payment { get; set; }
    public ShippingDto? Shipping { get; set; }
}

public class PaymentDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
}

public class CreateOrderRequest
{
    public int ProductId { get; set; }
    public int AddressId { get; set; }
    public int Quantity { get; set; }
    public string? PaymentMethod { get; set; }
}

public class UpdateOrderStatusRequest
{
    public string NewStatus { get; set; } = string.Empty; // processing, shipped, delivered, cancelled...
}

public class ShipOrderRequest
{
    public string Carrier { get; set; } = "GHN";
    public string TrackingNumber { get; set; } = string.Empty;
    public DateTime? EstimatedArrival { get; set; }
}

public class CancelOrderRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class ReturnRequestDto
{
    public string Reason { get; set; } = string.Empty;
}

public class PagedOrderResponse
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OrderResponse> Data { get; set; } = new List<OrderResponse>();
}

public class OrderFilterRequest
{
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AddressDto
{
    public int Id { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
}

public class ShippingDto
{
    public int Id { get; set; }
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Status { get; set; }
    public DateTime? EstimatedArrival { get; set; }
}