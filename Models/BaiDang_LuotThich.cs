using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_API.Models
{
    [Table("BaiDang_LuotThich")]
    public class BaiDang_LuotThich
    {
        [Key]
        public Guid id { get; set; }

        [ForeignKey("BaiDang")]
        public Guid baidang_id { get; set; }

        [ForeignKey("ApplicationUser")]
        public string? nguoidung_id { get; set; }

        public DateTime ngay_thich { get; set; }

        // Navigation properties
        public virtual BaiDang? BaiDang { get; set; }
        public virtual ApplicationUser? ApplicationUser { get; set; }
    }
}
