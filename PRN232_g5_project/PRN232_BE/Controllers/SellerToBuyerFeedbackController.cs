using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_BE;
using PRN232_BE.Models;
using Microsoft.AspNetCore.Hosting;

namespace PRN232_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SellerToBuyerFeedbackController : ControllerBase
    {
        private readonly CloneEbayDb1Context _context;
        private readonly IWebHostEnvironment _env;

        public SellerToBuyerFeedbackController(CloneEbayDb1Context context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private int GetCurrentUserId() => int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        public class CreateSellerToBuyerFeedbackRequest
        {
            public int BuyerId { get; set; }
            public int ProductId { get; set; }
            public string Message { get; set; } = string.Empty; // "xem và khen only" — nội dung do seller nhập
        }

        // Seller gửi feedback trực tiếp cho buyer (xem & khen)
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateSellerToBuyerFeedbackRequest request, [FromForm] List<IFormFile>? images)
        {
            var sellerId = GetCurrentUserId();

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId);
            if (product == null)
                return BadRequest("Product không tồn tại.");

            if (product.SellerId != sellerId)
                return Forbid("Chỉ seller của sản phẩm này mới được gửi feedback cho buyer.");

            var buyer = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.BuyerId);
            if (buyer == null)
                return BadRequest("Buyer không tồn tại.");

            // Tạo SellerToBuyerFeedback
            var fb = new SellerToBuyerFeedback
            {
                SellerId = sellerId,
                BuyerId = request.BuyerId,
                ProductId = request.ProductId,
                Message = request.Message,
                CreatedAt = DateTime.UtcNow
            };

            _context.SellerToBuyerFeedbacks.Add(fb);
            await _context.SaveChangesAsync(); // để có fb.Id

            if (images?.Any() == true)
                await SaveImages(images, sellerToBuyerFeedbackId: fb.Id);

            return Ok(new { Message = "Gửi feedback đến buyer thành công", SellerToBuyerFeedbackId = fb.Id });
        }

        // Helper: lưu ảnh (tương tự)
        private async Task SaveImages(List<IFormFile> files, int? feedbackDetailId = null, int? feedbackReplyId = null, int? sellerToBuyerFeedbackId = null)
        {
            var uploadsRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "feedbacks");
            if (!Directory.Exists(uploadsRoot))
                Directory.CreateDirectory(uploadsRoot);

            foreach (var file in files)
            {
                if (file.Length <= 0) continue;
                var fileName = $"{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadsRoot, fileName);
                await using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                }

                var url = $"/uploads/feedbacks/{fileName}";
                var img = new Image
                {
                    ImageUrl = url,
                    UploadedAt = DateTime.UtcNow,
                    FeedbackDetailId = feedbackDetailId,
                    FeedbackReplyId = feedbackReplyId,
                    SellerToBuyerFeedbackId = sellerToBuyerFeedbackId
                };
                _context.Images.Add(img);
            }

            await _context.SaveChangesAsync();
        }
    }
}