using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_API.Models
{
    /// <summary>
    /// Health/Diet Plan cho user
    /// </summary>
    [Table("HealthPlan")]
    public class HealthPlan
    {
        [Key]
        public int Id { get; set; }

        [StringLength(450)]
        public string? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? ApplicationUser { get; set; }

        /// <summary>
        /// Bệnh lý / Tình trạng sức khỏe
        /// </summary>
        [StringLength(500)]
        public string? BenhLy { get; set; }

        /// <summary>
        /// Thông tin phác đồ chi tiết
        /// </summary>
        [StringLength(2000)]
        public string? PhacDoText { get; set; }

        /// <summary>
        /// Thông tin dinh dưỡng (định dạng text)
        /// </summary>
        [StringLength(500)]
        public string? DinhDuong { get; set; }

        /// <summary>
        /// Những loại thức ăn được khuyến khích
        /// </summary>
        [StringLength(1000)]
        public string? KhuyenNghiMonAn { get; set; }

        /// <summary>
        /// Thời gian điều trị
        /// </summary>
        [StringLength(200)]
        public string? ThoiGianDieuTri { get; set; }

        /// <summary>
        /// Ngày tạo
        /// </summary>
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    }
}
