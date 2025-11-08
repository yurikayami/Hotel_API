namespace Hotel_API.Services
{
    /// <summary>
    /// Service để xử lý đường dẫn media, chuyển đổi đường dẫn tương đối thành URL đầy đủ
    /// </summary>
    public class MediaUrlService
    {
        private readonly IConfiguration _configuration;
        private readonly string _hotelWebBaseUrl;

        public MediaUrlService(IConfiguration configuration)
        {
            _configuration = configuration;
            _hotelWebBaseUrl = _configuration["HotelWebUrl"] ?? "https://localhost:7043";
        }

        /// <summary>
        /// Chuyển đổi đường dẫn media thành URL đầy đủ
        /// </summary>
        /// <param name="mediaPath">Đường dẫn media từ database</param>
        /// <returns>URL đầy đủ hoặc giữ nguyên nếu là base64/URL</returns>
        public string GetFullMediaUrl(string? mediaPath)
        {
            if (string.IsNullOrWhiteSpace(mediaPath))
            {
                return string.Empty;
            }

            // Nếu là base64, giữ nguyên
            if (mediaPath.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            {
                return mediaPath;
            }

            // Nếu đã là URL đầy đủ (http/https), giữ nguyên
            if (mediaPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                mediaPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return mediaPath;
            }

            // Nếu là đường dẫn tương đối (bắt đầu với /uploads), chuyển thành URL đầy đủ
            if (mediaPath.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase) ||
                mediaPath.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
            {
                // Đảm bảo có / ở đầu
                var relativePath = mediaPath.StartsWith("/") ? mediaPath : "/" + mediaPath;
                return $"{_hotelWebBaseUrl}{relativePath}";
            }

            // Các trường hợp khác, giữ nguyên
            return mediaPath;
        }

        /// <summary>
        /// Chuyển đổi danh sách đường dẫn media thành URL đầy đủ
        /// </summary>
        /// <param name="mediaPaths">Danh sách đường dẫn media</param>
        /// <returns>Danh sách URL đầy đủ</returns>
        public List<string> GetFullMediaUrls(IEnumerable<string?> mediaPaths)
        {
            return mediaPaths
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Select(path => GetFullMediaUrl(path))
                .ToList();
        }
    }
}
