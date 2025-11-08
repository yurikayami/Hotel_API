using Microsoft.AspNetCore.Identity;

namespace Hotel_API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? gioi_tinh { get; set; }
        public int? tuoi { get; set; }
        public string? ProfilePicture { get; set; }
        public bool? isFacebookLinked { get; set; }
        public bool? isGoogleLinked { get; set; }
        public bool? dang_online { get; set; }
        public string? googleProfilePicture { get; set; }
        public string? facebookProfilePicture { get; set; }
        public int? trang_thai { get; set; }
        public DateTime? lan_hoat_dong_cuoi { get; set; }
        public string? displayName { get; set; }
        public string? avatarUrl { get; set; }
        public int? kinh_nghiem { get; set; }
        public Guid? chuyenKhoaId { get; set; }
    }
}
