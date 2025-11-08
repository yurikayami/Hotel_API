using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_API.Models
{
    [Table("BinhLuan")]
    public class BinhLuan
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [ForeignKey("BaiDang")]
        [Column("bai_dang_id")]
        public Guid BaiDangId { get; set; }

        [ForeignKey("ApplicationUser")]
        [Column("id_nguoi_dung")]
        public string? NguoiDungId { get; set; }

        [Required]
        [Column("nguoi_binh_luan")]
        public string? NguoiBinhLuan { get; set; }

        [MaxLength]
        [Column("noi_dung")]
        public string? NoiDung { get; set; }

        [Column("ngay_binh_luan")]
        public DateTime NgayTao { get; set; }

        [Column("parent_comment_id")]
        public Guid? ParentCommentId { get; set; }

        // Navigation properties
        public virtual BaiDang? BaiDang { get; set; }
        public virtual ApplicationUser? ApplicationUser { get; set; }
    }
}
