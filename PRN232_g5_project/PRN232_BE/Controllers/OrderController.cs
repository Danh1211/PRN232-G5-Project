using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_BE;
using PRN232_BE.DTOs;
using PRN232_BE.Models;



[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly CloneEbayDb1Context _context;

    public OrderController(CloneEbayDb1Context context)
    {
        _context = context;
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirst("UserId")?.Value ?? "0");

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var userId = GetCurrentUserId();

        if (request.Quantity <= 0)
            return BadRequest("Quantity phải > 0");

        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == userId);

        if (address == null)
            return BadRequest("Địa chỉ không hợp lệ");

        var product = await _context.Products.FindAsync(request.ProductId);
        if (product == null)
            return BadRequest("Sản phẩm không tồn tại");

        var buyer = await _context.Users.FindAsync(userId);
        if (buyer == null)
            return Unauthorized("Không tìm thấy người dùng");

        var totalPrice = product.Price * request.Quantity;

        if (buyer.Balance < totalPrice)
            return BadRequest("Không đủ tiền trong tài khoản");

        var validMethods = new[] { "credit_card", "paypal", "bank_transfer" };
        var paymentMethod = (request.PaymentMethod?.ToLower() ?? "credit_card").Trim();

        if (!validMethods.Contains(paymentMethod))
            return BadRequest("Payment method không hợp lệ");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var order = new OrderTable
            {
                BuyerId = userId,
                SellerId = product.SellerId,
                AddressId = request.AddressId,
                ProductId = request.ProductId,
                OrderDate = DateTime.UtcNow,
                Amount = request.Quantity,
                Status = "shipped"   //->changes sang shipped
            };

            _context.OrderTables.Add(order);
            await _context.SaveChangesAsync();        // Lưu để có Order Id

            var payment = new Payment
            {
                OrderId = order.Id,
                BuyerId = userId,
                Amount = totalPrice,
                Method = paymentMethod,
                Status = "pending",
                PaidAt = DateTime.UtcNow
            };
            _context.Payments.Add(payment);

            var shipping = new ShippingInfo
            {
                OrderId = order.Id,
                Carrier = "GHN",
                Status = "shipped",  //->changes sang shipped
                HasSignature = false,
                EstimatedArrival = DateTime.UtcNow.AddDays(7)
            };
            _context.ShippingInfos.Add(shipping);

            // Escrow: trừ tiền buyer
            buyer.Balance -= totalPrice;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // ==================== FIX RESPONSE Ở ĐÂY ====================
            return Ok(new OrderResponse
            {
                Id = order.Id,
                ProductTitle = product.Title,
                Quantity = request.Quantity,
                TotalPrice = totalPrice,
                Status = order.Status,
                OrderDate = order.OrderDate,

                // Tránh null propagating error + trả về thông tin hữu ích hơn
                SellerUsername = product.SellerId > 0 ?
                    (await _context.Users.FindAsync(product.SellerId))?.Username : null,

                BuyerUsername = buyer.Username,

                Address = new AddressDto
                {
                    Id = address.Id,
                    Street = address.Street,
                    City = address.City,
                    Country = address.Country
                },

                Payment = new PaymentDto
                {
                    Id = payment.Id,
                    Amount = payment.Amount,
                    Method = payment.Method,
                    Status = payment.Status,
                    PaidAt = payment.PaidAt
                },

                Shipping = new ShippingDto
                {
                    Id = shipping.Id,
                    Carrier = shipping.Carrier,
                    TrackingNumber = shipping.TrackingNumber,
                    Status = shipping.Status,
                    EstimatedArrival = shipping.EstimatedArrival
                }
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            // Nên log lỗi ở production
            return StatusCode(500, $"Lỗi hệ thống khi tạo order: {ex.Message}");
        }
    }

    // ===============================
    // 2. GET ORDER DETAIL (mới)
    // ===============================
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderResponse>> GetOrderDetail(int id)
    {
        var userId = GetCurrentUserId();

        var order = await _context.OrderTables
            .AsNoTracking()
            .Include(o => o.Product)
            .Include(o => o.Payments)
            .Include(o => o.ShippingInfos)
            .Include(o => o.Address)
            .Include(o => o.Buyer)   // giả sử có navigation Buyer
            .Include(o => o.Seller)  // giả sử có navigation Seller
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound("Order không tồn tại");

        // Kiểm tra quyền: chỉ buyer hoặc seller của order này
        if (order.BuyerId != userId && order.SellerId != userId)
            return Forbid("Bạn không có quyền xem order này");

        var response = new OrderResponse
        {
            Id = order.Id,
            ProductTitle = order.Product.Title,
            Quantity = order.Amount,
            TotalPrice = order.Payments.FirstOrDefault()?.Amount ?? 0,
            Status = order.Status,
            OrderDate = order.OrderDate,
            SellerUsername = order.Seller?.Username,
            BuyerUsername = order.Buyer?.Username,
            Address = new AddressDto
            {
                Id = order.Address.Id,
                Street = order.Address.Street,
                City = order.Address.City,
                Country = order.Address.Country
            },
            Payment = order.Payments.Select(p => new PaymentDto
            {
                Id = p.Id,
                Amount = p.Amount,
                Method = p.Method,
                Status = p.Status,
                PaidAt = p.PaidAt
            }).FirstOrDefault(),
            Shipping = order.ShippingInfos
                .Select(s => new ShippingDto
                {
                    Id = s.Id,
                    Carrier = s.Carrier,
                    TrackingNumber = s.TrackingNumber,
                    Status = s.Status,
                    EstimatedArrival = s.EstimatedArrival
                })
                .FirstOrDefault()
            };

        return Ok(response);
    }

    // ================================
    // GET ORDERS BY BUYER & SELLER (TỐI ƯU + ĐẦY ĐỦ DTO)
    // ================================
    [HttpGet("buyer")]
    [HttpGet("seller")]
    public async Task<ActionResult<PagedOrderResponse>> GetOrders([FromQuery] OrderFilterRequest filter)
    {
        var userId = GetCurrentUserId();
        var isBuyer = Request.Path.Value?.Contains("buyer", StringComparison.OrdinalIgnoreCase) == true;

        var query = _context.OrderTables
            .AsNoTracking()
            .AsSplitQuery()                    // Quan trọng: tránh Cartesian explosion
            .Include(o => o.Product)
            .Include(o => o.Seller)
            .Include(o => o.Buyer)
            .Include(o => o.Address)
            .Include(o => o.Payments)
            .Include(o => o.ShippingInfos)
            .Where(o => isBuyer ? o.BuyerId == userId : o.SellerId == userId);

        // === Filters ===
        if (!string.IsNullOrEmpty(filter.Status))
            query = query.Where(o => o.Status.ToLower() == filter.Status.ToLower());

        if (filter.FromDate.HasValue)
            query = query.Where(o => o.OrderDate >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(o => o.OrderDate <= filter.ToDate.Value);

        var total = await query.CountAsync();

        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(o => new OrderResponse
            {
                Id = o.Id,
                ProductTitle = o.Product != null ? o.Product.Title : string.Empty,
                Quantity = o.Amount,
                TotalPrice = o.Payments.FirstOrDefault() != null ? o.Payments.First().Amount : 0,
                Status = o.Status ?? string.Empty,
                OrderDate = o.OrderDate,

                // Sửa lỗi null propagating ở đây
                SellerUsername = o.Seller != null ? o.Seller.Username : null,
                BuyerUsername = o.Buyer != null ? o.Buyer.Username : null,

                // Address
                Address = o.Address != null ? new AddressDto
                {
                    Id = o.Address.Id,
                    Street = o.Address.Street,
                    City = o.Address.City,
                    Country = o.Address.Country
                } : null,

                // Payment
                Payment = o.Payments.FirstOrDefault() != null ? new PaymentDto
                {
                    Id = o.Payments.First().Id,
                    Amount = o.Payments.First().Amount,
                    Method = o.Payments.First().Method,
                    Status = o.Payments.First().Status,
                    PaidAt = o.Payments.First().PaidAt
                } : null,

                // Shipping
                Shipping = o.ShippingInfos.FirstOrDefault() != null ? new ShippingDto
                {
                    Id = o.ShippingInfos.First().Id,
                    Carrier = o.ShippingInfos.First().Carrier,
                    TrackingNumber = o.ShippingInfos.First().TrackingNumber,
                    Status = o.ShippingInfos.First().Status,
                    EstimatedArrival = o.ShippingInfos.First().EstimatedArrival
                } : null
            })
            .ToListAsync();

        var response = new PagedOrderResponse
        {
            Total = total,
            Page = filter.Page,
            PageSize = filter.PageSize,
            Data = orders
        };

        return Ok(response);
    }

    // ===============================
    // 5. UPDATE ORDER STATUS (chủ yếu seller)
    // ===============================
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        var userId = GetCurrentUserId();
        var order = await _context.OrderTables
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();
        if (order.SellerId != userId) return Forbid("Chỉ seller mới được cập nhật status");

        var allowedTransitions = new Dictionary<string, string[]>
        {
            { "pending", new[] { "processing", "cancelled" } },
            { "processing", new[] { "shipped", "cancelled" } },
            { "shipped", new[] { "delivered", "cancelled" } }
        };

        if (!allowedTransitions.ContainsKey(order.Status) ||
            !allowedTransitions[order.Status].Contains(request.NewStatus))
            return BadRequest($"Không thể chuyển từ {order.Status} sang {request.NewStatus}");

        order.Status = request.NewStatus;

        if (request.NewStatus == "cancelled")
            await RefundPayment(order);

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Cập nhật status thành công", NewStatus = order.Status });
    }

    // ===============================
    // 6. MARK AS SHIPPED + Tracking (seller) -update orderstatus sang shipped
    // ===============================
    //[HttpPost("{id}/ship")]
    //public async Task<IActionResult> ShipOrder(int id, [FromBody] ShipOrderRequest request)
    //{
    //    var userId = GetCurrentUserId();
    //    var order = await _context.OrderTables
    //        .Include(o => o.ShippingInfos)
    //        .FirstOrDefaultAsync(o => o.Id == id);

    //    if (order == null) return NotFound();
    //    if (order.SellerId != userId) return Forbid("Chỉ seller mới được cập nhật status");

    //    if (order.Status != "processing")
    //        return BadRequest("Order phải ở trạng thái processing mới được ship");

    //    var shipping = order.ShippingInfos.FirstOrDefault();
    //    if (shipping == null) return NotFound("Không tìm thấy shipping info");

    //    shipping.Carrier = request.Carrier;
    //    shipping.TrackingNumber = request.TrackingNumber;
    //    shipping.Status = "shipped";
    //    shipping.EstimatedArrival = request.EstimatedArrival ?? DateTime.UtcNow.AddDays(5);

    //    order.Status = "shipped";

    //    await _context.SaveChangesAsync();
    //    return Ok(new { Message = "Đã đánh dấu shipped và cập nhật tracking" });
    //}

    // ===============================
    // 7. BUYER CONFIRM RECEIVED (release tiền cho seller)
    // ===============================
    [HttpPost("{id}/confirm-received")]
    public async Task<IActionResult> ConfirmReceived(int id)
    {
        var userId = GetCurrentUserId();
        var order = await _context.OrderTables
            .Include(o => o.Payments)
            .Include(o => o.Seller) // để cộng balance
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();
        if (order.BuyerId != userId) return Forbid();

        if (order.Status != "shipped")
            return BadRequest("Chỉ có thể confirm khi order đã shipped");

        order.Status = "delivered";

        var payment = order.Payments.FirstOrDefault();
        if (payment != null)
        {
            payment.Status = "completed";
            payment.PaidAt = DateTime.UtcNow;

            // Release tiền cho seller (escrow)
            var seller = order.Seller;
            if (seller != null)
                seller.Balance += payment.Amount;
        }

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Xác nhận đã nhận hàng thành công. Tiền đã chuyển cho seller." });
    }

    // ===============================
    // 8. CANCEL ORDER
    // ===============================
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(int id, [FromBody] CancelOrderRequest request)
    {
        var userId = GetCurrentUserId();
        var order = await _context.OrderTables
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        bool isBuyer = order.BuyerId == userId;
        bool isSeller = order.SellerId == userId;

        if (!isBuyer && !isSeller) return Forbid();

        if (order.Status is "shipped" or "delivered")
            return BadRequest("Không thể hủy order đã shipped/delivered");

        order.Status = "cancelled";

        await RefundPayment(order, request.Reason);

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Order đã được hủy và tiền hoàn lại cho buyer." });
    }

    // ===============================
    // 9. CREATE RETURN REQUEST (buyer)
    // ===============================
    [HttpPost("{id}/return")]
    public async Task<ActionResult> CreateReturnRequest(int id, [FromBody] ReturnRequestDto dto)
    {
        var userId = GetCurrentUserId();
        var order = await _context.OrderTables.FindAsync(id);
        if (order == null || order.BuyerId != userId) return Forbid();

        if (order.Status != "delivered")
            return BadRequest("Chỉ có thể yêu cầu return sau khi delivered");

        var returnReq = new ReturnRequest
        {
            OrderId = id,
            UserId = userId,
            Reason = dto.Reason,
            Status = "pending"
        };

        _context.ReturnRequests.Add(returnReq);
        await _context.SaveChangesAsync();

        return Ok(new { ReturnId = returnReq.Id, Message = "Yêu cầu return đã được gửi" });
    }

    // ===============================
    // 10. SELLER APPROVE / REJECT RETURN
    // ===============================
    [HttpPut("return/{returnId}/approve")]
    public async Task<IActionResult> ApproveReturn(int returnId)
    {
        var userId = GetCurrentUserId();
        var req = await _context.ReturnRequests
            .Include(r => r.Order)
            .ThenInclude(o => o.Payments)
            .FirstOrDefaultAsync(r => r.Id == returnId);

        if (req == null) return NotFound();
        if (req.Order.SellerId != userId) return Forbid();

        req.Status = "approved";
        req.Order.Status = "returned";

        await RefundPayment(req.Order, "Return approved");

        await _context.SaveChangesAsync();
        return Ok("Return đã được approve và tiền hoàn lại buyer");
    }

    [HttpPut("return/{returnId}/reject")]
    public async Task<IActionResult> RejectReturn(int returnId)
    {
        var userId = GetCurrentUserId();
        var req = await _context.ReturnRequests
            .Include(r => r.Order)
            .ThenInclude(o => o.Payments)
            .FirstOrDefaultAsync(r => r.Id == returnId);
        if (req == null) return NotFound();
        if (req.Order.SellerId != userId) return Forbid();

        req.Status = "rejected";
        await _context.SaveChangesAsync();
        return Ok("Return đã bị từ chối");
    }

    // ===============================
    // Helper: Refund Payment
    // ===============================
    private async Task RefundPayment(OrderTable order, string? reason = null)
    {
        var payment = order.Payments.FirstOrDefault();
        if (payment == null || payment.Status == "refunded") return;

        payment.Status = "refunded";

        // Hoàn tiền cho buyer
        var buyer = await _context.Users.FindAsync(order.BuyerId);
        if (buyer != null)
            buyer.Balance += payment.Amount;
    }

    // ===============================
    // 13. Refund manual (nếu cần admin hoặc dispute)
    // ===============================
    [Authorize(Roles = "admin")] // hoặc kiểm tra thêm
    [HttpPost("{id}/refund")]
    public async Task<IActionResult> ManualRefund(int orderid)
    {
        var order = await _context.OrderTables
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == orderid);

        if (order == null) return NotFound();

        await RefundPayment(order, "Manual refund by admin");
        order.Status = "refunded";

        await _context.SaveChangesAsync();
        return Ok("Đã hoàn tiền thủ công");
    }
}