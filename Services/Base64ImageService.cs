using System.Text.RegularExpressions;

namespace Hotel_API.Services
{
    /// <summary>
    /// Service xử lý ảnh Base64
    /// </summary>
    public class Base64ImageService
    {
        /// <summary>
        /// Xác thực và trích xuất Base64 string từ data URL
        /// </summary>
        public static string ExtractBase64(string base64Data)
        {
            if (string.IsNullOrEmpty(base64Data))
                throw new ArgumentException("Base64 string không được để trống");

            // Nếu có format "data:image/...;base64,..." thì lấy phần sau dấu phẩy
            if (base64Data.Contains(","))
            {
                base64Data = base64Data.Split(",")[1];
            }

            return base64Data.Trim();
        }

        /// <summary>
        /// Xác thực Base64 string
        /// </summary>
        public static bool IsValidBase64(string base64String)
        {
            try
            {
                Convert.FromBase64String(base64String);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lấy kích thước byte của Base64 string
        /// </summary>
        public static long GetBase64Size(string base64String)
        {
            return base64String.Length * 3 / 4;
        }

        /// <summary>
        /// Kiểm tra xem Base64 có phải ảnh hợp lệ không
        /// </summary>
        public static bool IsValidImageBase64(string base64String, string fileName)
        {
            // Kiểm tra kích thước
            const long maxSize = 5 * 1024 * 1024; // 5MB
            if (GetBase64Size(base64String) > maxSize)
                return false;

            // Kiểm tra extension
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(fileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
                return false;

            // Kiểm tra định dạng Base64
            if (!IsValidBase64(base64String))
                return false;

            // Kiểm tra magic bytes của hình ảnh
            try
            {
                var imageBytes = Convert.FromBase64String(base64String);
                return IsValidImageBytes(imageBytes);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra magic bytes của file ảnh
        /// </summary>
        private static bool IsValidImageBytes(byte[] bytes)
        {
            if (bytes.Length < 4)
                return false;

            // JPEG: FF D8 FF
            if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
                return true;

            // PNG: 89 50 4E 47
            if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
                return true;

            // GIF: 47 49 46
            if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46)
                return true;

            // WebP: 52 49 46 46 ... 57 45 42 50
            if (bytes.Length >= 12 &&
                bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46 &&
                bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50)
                return true;

            return false;
        }

        /// <summary>
        /// Chuyển đổi Base64 sang ảnh byte array
        /// </summary>
        public static byte[] ConvertBase64ToBytes(string base64String)
        {
            return Convert.FromBase64String(base64String);
        }

        /// <summary>
        /// Chuyển đổi byte array sang Base64
        /// </summary>
        public static string ConvertBytesToBase64(byte[] imageBytes)
        {
            return Convert.ToBase64String(imageBytes);
        }

        /// <summary>
        /// Lấy MIME type từ file extension
        /// </summary>
        public static string GetMimeType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }

        /// <summary>
        /// Tạo Data URL từ Base64
        /// </summary>
        public static string CreateDataUrl(string base64String, string fileName)
        {
            var mimeType = GetMimeType(fileName);
            return $"data:{mimeType};base64,{base64String}";
        }
    }
}
