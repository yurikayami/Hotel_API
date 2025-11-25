using System.ComponentModel.DataAnnotations;

namespace Hotel_API.Models.ViewModels
{
    /// <summary>
    /// DTO cho upload ảnh bài thuốc
    /// </summary>
    public class CreateBaiThuocDto
    {
        [Required(ErrorMessage = "Tên không được để trống")]
        [MaxLength(500)]
        public string Ten { get; set; } = string.Empty;

        [MaxLength(5000)]
        public string? MoTa { get; set; }

        [MaxLength(5000)]
        public string? HuongDanSuDung { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ảnh")]
        public IFormFile? Image { get; set; }
    }

    /// <summary>
    /// DTO response upload file
    /// </summary>
    public class FileUploadResponse
    {
        public string Filename { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
