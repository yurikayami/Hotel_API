using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_API.Models
{
    /// <summary>
    /// Lịch sử phân tích ảnh món ăn bằng AI
    /// </summary>
    [Table("PredictionHistory")]
    public class PredictionHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public required string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? ApplicationUser { get; set; }

        /// <summary>
        /// Đường dẫn tới ảnh
        /// </summary>
        [Required]
        [StringLength(500)]
        public required string ImagePath { get; set; }

        /// <summary>
        /// Tên món ăn dự đoán
        /// </summary>
        [Required]
        [StringLength(200)]
        public required string FoodName { get; set; }

        /// <summary>
        /// Độ chính xác của dự đoán (0-1)
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Calo tổng cộng
        /// </summary>
        public double Calories { get; set; }

        /// <summary>
        /// Protein (grams)
        /// </summary>
        public double Protein { get; set; }

        /// <summary>
        /// Fat (grams)
        /// </summary>
        public double Fat { get; set; }

        /// <summary>
        /// Carbs (grams)
        /// </summary>
        public double Carbs { get; set; }

        /// <summary>
        /// Loại bữa ăn (breakfast, lunch, dinner, snack)
        /// </summary>
        [StringLength(50)]
        public string? MealType { get; set; }

        /// <summary>
        /// Lý do phân tích
        /// </summary>
        [StringLength(500)]
        public string? Reason { get; set; }

        /// <summary>
        /// Đánh giá phù hợp với phác đồ (0-100 %)
        /// </summary>
        public string? Suitable { get; set; }

        /// <summary>
        /// Đề xuất cải thiện
        /// </summary>
        [StringLength(500)]
        public string? Suggestions { get; set; }

        /// <summary>
        /// Lời khuyên từ AI
        /// </summary>
        [StringLength(500)]
        public string? Advice { get; set; }

        /// <summary>
        /// Thời gian tạo
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Chi tiết từng thành phần
        /// </summary>
        public virtual ICollection<PredictionDetail>? Details { get; set; } = new List<PredictionDetail>();
    }
}
