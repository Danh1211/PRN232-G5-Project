using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_BE;
using PRN232_BE.DTOs;
using PRN232_BE.Models;



[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly CloneEbayDb1Context _context;
    private readonly IWebHostEnvironment _env;   // Để lưu file vật lý

    public ReviewController(CloneEbayDb1Context context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirst("UserId")?.Value ?? "0");

    // ===============================
    // CREATE REVIEW + IMAGES (mới & cải tiến)
    // ===============================
    [Authorize]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ReviewResponse>> CreateReview(
        [FromForm] CreateReviewRequest request,
        [FromForm] List<IFormFile> images)   // hỗ trợ nhiều ảnh
    {
        var userId = GetCurrentUserId();

        if (request.Rating < 1 || request.Rating > 5)
            return BadRequest("Rating phải từ 1 đến 5");

        var order = await _context.OrderTables
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.BuyerId == userId);

        // Load luôn User để lấy Username
        var currentUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (currentUser == null)
            return Unauthorized("User không tồn tại");

        if (order == null)
            return BadRequest("Order không hợp lệ hoặc không phải của bạn");

        if (order.Status != "delivered" && order.Status != "completed")   // tôi sửa thành delivered cho thực tế
            return BadRequest("Chỉ được review khi order đã delivered");

        var exists = await _context.Reviews.AnyAsync(r => r.OrderId == request.OrderId);
        if (exists)
            return BadRequest("Bạn đã review order này rồi");

        var review = new Review
        {
            OrderId = request.OrderId,
            ProductId = order.ProductId,
            ReviewerId = userId,
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();   // lưu review trước để có Id

        // Xử lý upload ảnh
        var imageUrls = await SaveReviewImages(review.Id, images);

        await _context.SaveChangesAsync();

        return Ok(new ReviewResponse
        {
            Id = review.Id,
            Rating = review.Rating,
            Comment = review.Comment,
            ReviewerName = currentUser.Username,
            CreatedAt = review.CreatedAt,
            ImageUrls = imageUrls
        });
    }

    // Helper: lưu ảnh và tạo record trong bảng Image
    private async Task<List<string>> SaveReviewImages(int reviewId, List<IFormFile> files)
    {
        var imageUrls = new List<string>();
        if (files == null || files.Count == 0) return imageUrls;

        var uploadFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "reviews");
        if (!Directory.Exists(uploadFolder))
            Directory.CreateDirectory(uploadFolder);

        foreach (var file in files)
        {
            if (file.Length > 5 * 1024 * 1024) // giới hạn 5MB
                continue;

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(uploadFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var imageUrl = $"/uploads/reviews/{fileName}";

            var imageEntity = new Image
            {
                ImageUrl = imageUrl,
                UploadedAt = DateTime.UtcNow,
                ReviewId = reviewId   // liên kết với review
            };

            _context.Images.Add(imageEntity);
            imageUrls.Add(imageUrl);
        }

        return imageUrls;
    }

    // ===============================
    // GET REVIEWS BY PRODUCT - Có phân trang + sắp xếp + ảnh
    // ===============================
    [HttpGet("product/{productId}")]
    public async Task<ActionResult<PagedReviewResponse>> GetReviewsByProduct(
        int productId,
        [FromQuery] ReviewQuery query)
    {
        var reviewsQuery = _context.Reviews
            .AsNoTracking()
            .Include(r => r.Reviewer)
            .Include(r => r.Images)
            .Where(r => r.ProductId == productId);

        // Sorting
        reviewsQuery = query.SortBy?.ToLower() switch
        {
            "oldest" => reviewsQuery.OrderBy(r => r.CreatedAt),
            "rating" => reviewsQuery.OrderByDescending(r => r.Rating),
            _ => reviewsQuery.OrderByDescending(r => r.CreatedAt)   // mặc định newest
        };

        var total = await reviewsQuery.CountAsync();

        var data = await reviewsQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(r => new ReviewResponse
            {
                Id = r.Id,
                Rating = r.Rating,
                Comment = r.Comment,
                ReviewerName = r.Reviewer != null ? r.Reviewer.Username : "Anonymous",
                CreatedAt = r.CreatedAt,
                ImageUrls = r.Images.Select(i => i.ImageUrl).ToList()
            })
            .ToListAsync();

        return Ok(new PagedReviewResponse
        {
            Total = total,
            Page = query.PageNumber,
            PageSize = query.PageSize,
            Data = data
        });
    }


    // GET MY REVIEWS (cập nhật có ảnh)
    [Authorize]
    [HttpGet("my-reviews")]
    public async Task<ActionResult<IEnumerable<ReviewResponse>>> GetMyReviews()
    {
        var userId = GetCurrentUserId();

        var data = await _context.Reviews
            .Include(r => r.Product)
            .Include(r => r.Images)
            .Where(r => r.ReviewerId == userId)
            .Select(r => new ReviewResponse
            {
                Id = r.Id,
                Rating = r.Rating,
                Comment = r.Comment,
                ReviewerName = r.Reviewer.Username,
                CreatedAt = r.CreatedAt,
                ImageUrls = r.Images.Select(i => i.ImageUrl).ToList()
            })
            .ToListAsync();

        return Ok(data);
    }

    // UPDATE REVIEW + thay ảnh mới (nếu có)
    [Authorize]
    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateReview(
        int id,
        [FromForm] CreateReviewRequest request,
        [FromForm] List<IFormFile>? images = null)
    {
        var userId = GetCurrentUserId();
        var review = await _context.Reviews
            .Include(r => r.Images)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review == null) return NotFound();
        if (review.ReviewerId != userId) return Forbid();

        if ((DateTime.UtcNow - review.CreatedAt).TotalDays > 7)
            return BadRequest("Chỉ được sửa review trong 7 ngày");

        if (request.Rating < 1 || request.Rating > 5)
            return BadRequest("Rating phải từ 1 đến 5");

        review.Rating = request.Rating;
        review.Comment = request.Comment;

        // Nếu có ảnh mới → xóa ảnh cũ và thêm mới
        if (images != null && images.Any())
        {
            // Xóa file vật lý và record cũ
            foreach (var oldImg in review.Images)
            {
                var filePath = Path.Combine(_env.WebRootPath ?? "wwwroot", oldImg.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                _context.Images.Remove(oldImg);
            }

            var newUrls = await SaveReviewImages(review.Id, images);
        }

        await _context.SaveChangesAsync();
        return Ok("Cập nhật review thành công");
    }

    // DELETE REVIEW + xóa ảnh
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var userId = GetCurrentUserId();
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var review = await _context.Reviews
            .Include(r => r.Images)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review == null) return NotFound();

        if (role != "admin" && review.ReviewerId != userId)
            return Forbid();

        // Xóa file vật lý
        foreach (var img in review.Images)
        {
            var filePath = Path.Combine(_env.WebRootPath ?? "wwwroot", img.ImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return Ok("Đã xóa review và các ảnh liên quan");
    }

    // GET SUMMARY giữ nguyên (không ảnh)
    [HttpGet("product/{productId}/summary")]
    public async Task<IActionResult> GetSummary(int productId)
    {
        // code cũ của bạn, giữ nguyên...
        var reviews = await _context.Reviews
            .Where(r => r.ProductId == productId)
            .ToListAsync();

        if (!reviews.Any())
            return Ok(new { averageRating = 0, totalReviews = 0, ratingDistribution = new int[5] });

        var total = reviews.Count;
        var avg = reviews.Average(r => r.Rating);
        var distribution = new int[5];
        foreach (var r in reviews)
            distribution[r.Rating - 1]++;

        return Ok(new
        {
            averageRating = Math.Round(avg, 1),
            totalReviews = total,
            ratingDistribution = distribution
        });
    }
}