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
    public class FeedbackController : ControllerBase
    {
        private readonly CloneEbayDb1Context _context;
        private readonly IWebHostEnvironment _env;

        public FeedbackController(CloneEbayDb1Context context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private int GetCurrentUserId() => int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        // DTOs local to controller
        public class CreateFeedbackDetailRequest
        {
            public int OrderId { get; set; }
            public byte Type { get; set; } // ví dụ: 0 = neutral, 1 = positive, 2 = negative
            public byte? ItemAsDescribed { get; set; }
            public byte? Communication { get; set; }
            public byte? ShippingTime { get; set; }
            public byte? ShippingCost { get; set; }
            public string? Comment { get; set; }
        }

        public class UpdateFeedbackDetailRequest
        {
            public byte? ItemAsDescribed { get; set; }
            public byte? Communication { get; set; }
            public byte? ShippingTime { get; set; }
            public byte? ShippingCost { get; set; }
            public string? Comment { get; set; }
            public byte? Type { get; set; }
        }

        // Tạo FeedbackDetail (buyer)
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateFeedbackDetail([FromForm] CreateFeedbackDetailRequest request, [FromForm] List<IFormFile>? images)
        {
            var userId = GetCurrentUserId();

            var order = await _context.OrderTables
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.BuyerId == userId);

            if (order == null)
                return BadRequest("Order không hợp lệ hoặc không phải của bạn.");

            if (order.Status != "delivered" && order.Status != "completed")
                return BadRequest("Chỉ được đánh giá khi order đã delivered hoặc completed.");

            var exists = await _context.FeedbackDetails.AnyAsync(fd => fd.OrderId == request.OrderId);
            if (exists)
                return BadRequest("Bạn đã gửi feedback cho order này rồi.");

            // Lấy hoặc tạo Feedback tổng cho seller
            var feedback = await _context.Feedbacks.FirstOrDefaultAsync(f => f.SellerId == order.SellerId);
            if (feedback == null)
            {
                feedback = new Feedback { SellerId = order.SellerId, AverageRating = 0, PositiveRate = 0, NegativeRate = 0 };
                _context.Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();
            }

            var detail = new FeedbackDetail
            {
                FeedbackId = feedback.Id,
                BuyerId = userId,
                OrderId = request.OrderId,
                Type = request.Type,
                ItemAsDescribed = request.ItemAsDescribed,
                Communication = request.Communication,
                ShippingTime = request.ShippingTime,
                ShippingCost = request.ShippingCost,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.FeedbackDetails.Add(detail);
            await _context.SaveChangesAsync(); // để có detail.Id

            // Lưu ảnh nếu có
            if (images?.Any() == true)
                await SaveImages(images, feedbackDetailId: detail.Id);

            // Recalculate seller summary
            await RecalculateFeedbackSummary(feedback.Id);

            return Ok(new { Message = "Gửi feedback thành công", FeedbackDetailId = detail.Id });
        }

        // Cập nhật FeedbackDetail (chỉ được update 1 lần trước khi shop trả lời)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFeedbackDetail(int id, [FromBody] UpdateFeedbackDetailRequest request)
        {
            var userId = GetCurrentUserId();

            var detail = await _context.FeedbackDetails
                .Include(d => d.FeedbackReply)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (detail == null)
                return NotFound("FeedbackDetail không tồn tại.");

            if (detail.BuyerId != userId)
                return Forbid("Chỉ buyer tạo feedback mới được cập nhật.");

            // Quy tắc: không cho update nếu shop đã trả lời (chỉ 1 lần trước khi shop reply)
            if (detail.FeedbackReply != null)
                return BadRequest("Không thể cập nhật feedback sau khi shop đã trả lời.");

            // Thực hiện cập nhật (cho phép cập nhật 1 lần; vì db không có flag, ta cho phép cập nhật miễn là chưa reply)
            if (request.Comment != null) detail.Comment = request.Comment;
            if (request.ItemAsDescribed.HasValue) detail.ItemAsDescribed = request.ItemAsDescribed;
            if (request.Communication.HasValue) detail.Communication = request.Communication;
            if (request.ShippingTime.HasValue) detail.ShippingTime = request.ShippingTime;
            if (request.ShippingCost.HasValue) detail.ShippingCost = request.ShippingCost;
            if (request.Type.HasValue) detail.Type = request.Type.Value;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Cập nhật feedback thành công" });
        }

        // Helper: lưu ảnh vào wwwroot/uploads/feedbacks và tạo record Image
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