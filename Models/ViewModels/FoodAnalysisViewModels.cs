using System.ComponentModel.DataAnnotations;

namespace Hotel_API.Models.ViewModels
{
    /// <summary>
    /// DTO để phân tích ảnh món ăn bằng AI
    /// </summary>
    public class FoodAnalysisDto
    {
        [Required(ErrorMessage = "Dữ liệu ảnh không được để trống")]
        public string ImageBase64 { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên file không được để trống")]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Loại bữa ăn: breakfast, lunch, dinner, snack
        /// Mặc định: lunch
        /// </summary>
        public string? MealType { get; set; }
    }

    /// <summary>
    /// DTO response cho phân tích ảnh thành công
    /// </summary>
    public class FoodAnalysisResponseDto
    {
        public string ImageUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public PredictionDto? Prediction { get; set; }
        public PlanAdviceDto? PlanAdvice { get; set; }
    }

    /// <summary>
    /// DTO chứa kết quả dự đoán từ AI
    /// </summary>
    public class PredictionDto
    {
        public string PredictedLabel { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public NutritionDto? Nutrition { get; set; }
    }

    /// <summary>
    /// DTO chứa thông tin dinh dưỡng
    /// </summary>
    public class NutritionDto
    {
        /// <summary>
        /// Calories (kcal)
        /// </summary>
        public int Calories { get; set; }

        /// <summary>
        /// Protein (grams)
        /// </summary>
        public int Protein { get; set; }

        /// <summary>
        /// Carbohydrates (grams)
        /// </summary>
        public int Carbs { get; set; }

        /// <summary>
        /// Fat (grams)
        /// </summary>
        public int Fat { get; set; }

        /// <summary>
        /// Fiber (grams)
        /// </summary>
        public int Fiber { get; set; }

        /// <summary>
        /// Loại bữa ăn
        /// </summary>
        public string MealType { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO chứa lời khuyên dựa trên health plan của user
    /// </summary>
    public class PlanAdviceDto
    {
        /// <summary>
        /// Bữa ăn này có nằm trong giới hạn calories không
        /// </summary>
        public bool IsWithinCalorieLimit { get; set; }

        /// <summary>
        /// Calories còn lại trong ngày
        /// </summary>
        public int RemainingCalories { get; set; }

        /// <summary>
        /// Thông điệp lời khuyên
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Danh sách các lời khuyên cụ thể
        /// </summary>
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// DTO cho lịch sử phân tích
    /// </summary>
    public class AnalysisHistoryDto
    {
        public string Id { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string FoodName { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public int Calories { get; set; }
        public int Protein { get; set; }
        public int Fat { get; set; }
        public int Carbs { get; set; }
        public string MealType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? Advice { get; set; }
    }
}
