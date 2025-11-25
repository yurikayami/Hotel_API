using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_API.Models
{
    /// <summary>
    /// Model cho Món Ăn (Food Dishes)
    /// </summary>
    [Table("MonAn")]
    public class MonAn
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Ten { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? MoTa { get; set; }

        [MaxLength(5000)]
        public string? CachCheBien { get; set; }

        [MaxLength(100)]
        public string? Loai { get; set; } = "Chung";

        public DateTime? NgayTao { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Image { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Gia { get; set; }

        public int? SoNguoi { get; set; }

        public int LuotXem { get; set; } = 0;
    }
}
