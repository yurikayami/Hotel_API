using System.ComponentModel.DataAnnotations;

namespace Hotel_API.Models.ViewModels
{
    /// <summary>
    /// Request model for search queries
    /// </summary>
    public class SearchRequestDto
    {
        [Required(ErrorMessage = "Query không được để trống")]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Query phải từ 2-500 ký tự")]
        public string? Query { get; set; }

        [RegularExpression("^(all|users|posts|medicines|dishes)$", ErrorMessage = "Type không hợp lệ")]
        public string Type { get; set; } = "all";

        [Range(1, int.MaxValue, ErrorMessage = "Page phải >= 1")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Limit phải từ 1-100")]
        public int Limit { get; set; } = 20;
    }

    /// <summary>
    /// Response model for user search results
    /// </summary>
    public class UserSearchResultDto
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Avatar { get; set; }
        public string? DisplayName { get; set; }
    }

    /// <summary>
    /// Response model for post search results
    /// </summary>
    public class PostSearchResultDto
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Image { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? ViewCount { get; set; }
        public int? LikeCount { get; set; }
    }

    /// <summary>
    /// Response model for medicine search results
    /// </summary>
    public class MedicineSearchResultDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public int? ViewCount { get; set; }
        public int? LikeCount { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    /// <summary>
    /// Response model for dish search results
    /// </summary>
    public class DishSearchResultDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public decimal? Price { get; set; }
        public string? Category { get; set; }
        public int? Servings { get; set; }
        public int? ViewCount { get; set; }
    }

    /// <summary>
    /// General search response containing all types of results
    /// </summary>
    public class GeneralSearchResponseDto
    {
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
        public SearchDataDto? Data { get; set; }
        public PaginationDto? Pagination { get; set; }
    }

    /// <summary>
    /// Container for search results of all types
    /// </summary>
    public class SearchDataDto
    {
        public List<UserSearchResultDto> Users { get; set; } = new();
        public List<PostSearchResultDto> Posts { get; set; } = new();
        public List<MedicineSearchResultDto> Medicines { get; set; } = new();
        public List<DishSearchResultDto> Dishes { get; set; } = new();
    }

    /// <summary>
    /// Pagination information
    /// </summary>
    public class PaginationDto
    {
        public int Page { get; set; }
        public int Limit { get; set; }
        public int Total { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Type-specific search response
    /// </summary>
    public class TypedSearchResponseDto
    {
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
        public object? Data { get; set; }
        public PaginationDto? Pagination { get; set; }
    }

    /// <summary>
    /// Suggestions/Autocomplete response
    /// </summary>
    public class SuggestionsResponseDto
    {
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
        public SuggestionsDataDto? Data { get; set; }
    }

    /// <summary>
    /// Container for suggestions
    /// </summary>
    public class SuggestionsDataDto
    {
        public List<string> Suggestions { get; set; } = new();
    }

    /// <summary>
    /// Error response
    /// </summary>
    public class ErrorResponseDto
    {
        public bool Success { get; set; } = false;
        public string? Message { get; set; }
        public string? Code { get; set; }
    }
}
