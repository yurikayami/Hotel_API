using System.ComponentModel.DataAnnotations;

namespace Hotel_API.Models.ViewModels
{
    /// <summary>
    /// DTO cho form upload ảnh phân tích món ăn
    /// </summary>
    public class FoodAnalysisFormDto
    {
        /// <summary>
        /// File ảnh của món ăn
        /// </summary>
        [Required(ErrorMessage = "Ảnh là bắt buộc")]
        public IFormFile? Image { get; set; }

        /// <summary>
        /// ID của user
        /// </summary>
        [Required(ErrorMessage = "User ID là bắt buộc")]
        [StringLength(450)]
        public string? UserId { get; set; }

        /// <summary>
        /// Loại bữa ăn (breakfast, lunch, dinner, snack)
        /// </summary>
        [StringLength(50)]
        public string? MealType { get; set; }
    }
}
