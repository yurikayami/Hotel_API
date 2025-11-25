using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_API.Models
{
    /// <summary>
    /// Model cho Bài Thuốc (Medical Articles)
    /// </summary>
    [Table("BaiThuoc")]
    public class BaiThuoc
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Ten { get; set; } = string.Empty;

        [MaxLength(5000)]
        public string? MoTa { get; set; }

        [MaxLength(5000)]
        public string? HuongDanSuDung { get; set; }

        public string? NguoiDungId { get; set; }

        [ForeignKey(nameof(NguoiDungId))]
        public virtual ApplicationUser? ApplicationUser { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Image { get; set; }

        public int? SoLuotThich { get; set; } = 0;

        public int? SoLuotXem { get; set; } = 0;

        public int? TrangThai { get; set; } = 1; // 1: Active, 0: Inactive
    }
}
