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
    public class FeedbackReplyController : ControllerBase
    {
        private readonly CloneEbayDb1Context _context;
        private readonly IWebHostEnvironment _env;

        public FeedbackReplyController(CloneEbayDb1Context context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private int GetCurrentUserId() => int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        public class CreateFeedbackReplyRequest
        {
            public int FeedbackDetailId { get; set; }
            public string ReplyContent { get; set; } = string.Empty;
        }

        // Shop tạo reply cho feedback detail (chỉ 1 lần)
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateReply([FromForm] CreateFeedbackReplyRequest request, [FromForm] List<IFormFile>? images)
        {
            var userId = GetCurrentUserId();

            var detail = await _context.FeedbackDetails
                .Include(d => d.Order)
                .Include(d => d.Feedback)
                .FirstOrDefaultAsync(d => d.Id == request.FeedbackDetailId);

            if (detail == null)
                return NotFound("FeedbackDetail không tồn tại.");

            // Kiểm tra quyền: chỉ seller của order / feedback mới được trả lời
            var sellerId = detail.Feedback?.SellerId ?? detail.Order.SellerId;
            if (sellerId != userId)
                return Forbid("Chỉ shop liên quan mới được trả lời feedback này.");

            // Kiểm tra đã có reply chưa => chỉ cho 1 lần
            var exists = await _context.FeedbackReplies.AnyAsync(r => r.FeedbackDetailId == request.FeedbackDetailId);
            if (exists)
                return BadRequest("Feedback đã được trả lời trước đó.");

            var reply = new FeedbackReply
            {
                FeedbackDetailId = request.FeedbackDetailId,
                SellerId = userId,
                ReplyContent = request.ReplyContent,
                CreatedAt = DateTime.UtcNow
            };

            _context.FeedbackReplies.Add(reply);
            await _context.SaveChangesAsync(); // để có reply.Id

            if (images?.Any() == true)
                await SaveImages(images, feedbackReplyId: reply.Id);

            return Ok(new { Message = "Trả lời feedback thành công", FeedbackReplyId = reply.Id });
        }

        // Helper: lưu ảnh (tương tự FeedbackController)
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