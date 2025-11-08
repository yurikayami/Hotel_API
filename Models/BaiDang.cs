using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_API.Models
{
    [Table("BaiDang")]
    public class BaiDang
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("ApplicationUser")]
        public string? NguoiDungId { get; set; }

        [MaxLength]
        public string? NoiDung { get; set; }

        [MaxLength(50)]
        public string? Loai { get; set; }

        [MaxLength]
        public string? DuongDanMedia { get; set; }

        public DateTime? NgayDang { get; set; }

        public Guid? Id_MonAn { get; set; }

        public int? LuotThich { get; set; }

        public int? SoBinhLuan { get; set; }

        [MaxLength(100)]
        public string? NguoiDang { get; set; }

        public bool? DaDuyet { get; set; }

        public int so_chia_se { get; set; }

        [MaxLength]
        public string? hashtags { get; set; }

        [MaxLength]
        public string? keywords { get; set; }

        // Navigation properties
        public virtual ApplicationUser? ApplicationUser { get; set; }
    }
}
