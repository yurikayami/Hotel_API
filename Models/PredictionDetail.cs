using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_API.Models
{
    /// <summary>
    /// Chi tiết từng thành phần của dự đoán ảnh
    /// </summary>
    [Table("PredictionDetail")]
    public class PredictionDetail
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// FK đến PredictionHistory
        /// </summary>
        public int PredictionHistoryId { get; set; }

        [ForeignKey(nameof(PredictionHistoryId))]
        public virtual PredictionHistory PredictionHistory { get; set; }

        /// <summary>
        /// Tên thành phần (ví dụ: "cơm", "thịt gà")
        /// </summary>
        [StringLength(200)]
        public string Label { get; set; }

        /// <summary>
        /// Khối lượng của thành phần (grams)
        /// </summary>
        public double Weight { get; set; }

        /// <summary>
        /// Calo của thành phần
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
        /// Độ tin cậy của dự đoán (0-1)
        /// </summary>
        public double Confidence { get; set; }
    }
}
