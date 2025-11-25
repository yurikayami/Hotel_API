using System.ComponentModel.DataAnnotations;

namespace Hotel_API.Models.ViewModels
{
    /// <summary>
    /// DTO để tạo bài viết mới
    /// </summary>
    public class CreatePostDto
    {
        [Required(ErrorMessage = "Nội dung không được để trống")]
        [MaxLength(5000, ErrorMessage = "Nội dung tối đa 5000 ký tự")]
        public string NoiDung { get; set; } = string.Empty;

        public string? Loai { get; set; }

        /// <summary>
        /// Có thể là URL hoặc Base64 string của ảnh
        /// </summary>
        public string? DuongDanMedia { get; set; }

        public Guid? MonAnId { get; set; }

        public string? Hashtags { get; set; }
    }

    /// <summary>
    /// DTO để upload ảnh dưới dạng Base64
    /// </summary>
    public class ImageUploadDto
    {
        [Required(ErrorMessage = "Dữ liệu ảnh không được để trống")]
        public string ImageBase64 { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên file không được để trống")]
        public string FileName { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO response cho bài viết
    /// </summary>
    public class PostDto
    {
        public Guid Id { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public string? Loai { get; set; }
        public string? DuongDanMedia { get; set; }
        public DateTime? NgayDang { get; set; }
        public int? LuotThich { get; set; }
        public int? SoBinhLuan { get; set; }
        public int SoChiaSe { get; set; }
        public bool IsLiked { get; set; }
        public string? Hashtags { get; set; }

        // Thông tin người đăng
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorAvatar { get; set; }
    }

    /// <summary>
    /// DTO cho danh sách bài viết có phân trang
    /// </summary>
    public class PostPagedResult
    {
        public List<PostDto> Posts { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }

    /// <summary>
    /// DTO cho danh sách bài viết với infinity scroll (cursor-based)
    /// </summary>
    public class PostInfinityResult
    {
        public List<PostDto> Posts { get; set; } = new();
        public bool HasMore { get; set; }
    }

    /// <summary>
    /// DTO để tạo comment
    /// </summary>
    public class CreateCommentDto
    {
        [Required(ErrorMessage = "Nội dung comment không được để trống")]
        [MaxLength(1000, ErrorMessage = "Comment tối đa 1000 ký tự")]
        public string NoiDung { get; set; } = string.Empty;

        public Guid? ParentCommentId { get; set; }
    }

    /// <summary>
    /// DTO response cho comment
    /// </summary>
    public class CommentDto
    {
        public Guid Id { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public DateTime NgayTao { get; set; }
        public Guid? ParentCommentId { get; set; }

        // Thông tin người comment
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatar { get; set; }

        // Replies cho comment cha
        public List<CommentDto> Replies { get; set; } = new();
    }

    /// <summary>
    /// DTO cho danh sách bình luận có phân trang
    /// </summary>
    public class CommentPagedResult
    {
        public List<CommentDto> Comments { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }

    /// <summary>
    /// DTO cho danh sách bình luận với infinity scroll (cursor-based)
    /// </summary>
    public class CommentInfinityResult
    {
        public List<CommentDto> Comments { get; set; } = new();
        public bool HasMore { get; set; }
    }

    /// <summary>
    /// Response chuẩn cho API
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
