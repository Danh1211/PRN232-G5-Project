
// CreateReviewRequest - hỗ trợ upload file
public class CreateReviewRequest
{
    public int OrderId { get; set; }
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
}

// ReviewResponse - thêm danh sách ảnh
public class ReviewResponse
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<string> ImageUrls { get; set; } = new List<string>();   // ← thêm
}

// ReviewQuery giữ nguyên
public class ReviewQuery
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "newest";
}

public class PagedReviewResponse
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<ReviewResponse> Data { get; set; } = new();
}
