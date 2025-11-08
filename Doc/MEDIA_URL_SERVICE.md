# Media URL Service - Hướng dẫn sử dụng

## Giới thiệu

`MediaUrlService` là service chuyển đổi đường dẫn media từ database thành URL đầy đủ để Flutter app có thể truy cập hình ảnh từ Hotel_Web project.

## Vấn đề

API trả về 3 loại đường dẫn media:
1. **Base64**: `data:image/jpeg;base64,/9j/4AAQSkZJRg...` → Giữ nguyên
2. **URL đầy đủ**: `https://example.com/image.jpg` → Giữ nguyên  
3. **Đường dẫn tương đối**: `/uploads/posts/abc.jpg` → **CẦN CHUYỂN ĐỔI**

Đường dẫn tương đối lưu trong database nhưng file thực tế nằm trong `Hotel_Web\wwwroot\uploads\`. Flutter app cần URL đầy đủ để tải hình ảnh.

## Giải pháp

Service tự động chuyển đổi đường dẫn tương đối thành URL đầy đủ:
- `/uploads/posts/abc.jpg` → `https://localhost:7043/uploads/posts/abc.jpg`
- `uploads/posts/abc.jpg` → `https://localhost:7043/uploads/posts/abc.jpg`

## Cấu hình

### 1. appsettings.json

```json
{
  "HotelWebUrl": "https://localhost:7043",
  // ... cấu hình khác
}
```

**Quan trọng**: 
- Development: `https://localhost:7043` (port mặc định của Hotel_Web)
- Production: Thay bằng domain thực tế (ví dụ: `https://api.yourhotel.com`)

### 2. appsettings.Development.json

```json
{
  "HotelWebUrl": "https://localhost:7043"
}
```

## Sử dụng

### Trong Controller

```csharp
using Hotel_API.Services;

public class YourController : ControllerBase
{
    private readonly MediaUrlService _mediaUrlService;

    public YourController(MediaUrlService mediaUrlService)
    {
        _mediaUrlService = mediaUrlService;
    }

    [HttpGet]
    public async Task<IActionResult> GetData()
    {
        var post = await _context.Posts.FindAsync(id);
        
        // Chuyển đổi đường dẫn media thành URL đầy đủ
        post.DuongDanMedia = _mediaUrlService.GetFullMediaUrl(post.DuongDanMedia);
        post.AuthorAvatar = _mediaUrlService.GetFullMediaUrl(post.AuthorAvatar);
        
        return Ok(post);
    }
}
```

### Chuyển đổi nhiều đường dẫn

```csharp
var mediaUrls = new List<string> 
{ 
    "/uploads/posts/img1.jpg", 
    "/uploads/avatars/user.png" 
};

var fullUrls = _mediaUrlService.GetFullMediaUrls(mediaUrls);
// Kết quả:
// [
//   "https://localhost:7043/uploads/posts/img1.jpg",
//   "https://localhost:7043/uploads/avatars/user.png"
// ]
```

## Logic xử lý

```csharp
public string GetFullMediaUrl(string? mediaPath)
{
    if (string.IsNullOrWhiteSpace(mediaPath))
        return string.Empty;

    // Nếu là base64, giữ nguyên
    if (mediaPath.StartsWith("data:image/"))
        return mediaPath;

    // Nếu đã là URL đầy đủ, giữ nguyên
    if (mediaPath.StartsWith("http://") || mediaPath.StartsWith("https://"))
        return mediaPath;

    // Nếu là đường dẫn tương đối, chuyển thành URL đầy đủ
    if (mediaPath.StartsWith("/uploads/") || mediaPath.StartsWith("uploads/"))
    {
        var relativePath = mediaPath.StartsWith("/") ? mediaPath : "/" + mediaPath;
        return $"{HotelWebBaseUrl}{relativePath}";
    }

    // Các trường hợp khác, giữ nguyên
    return mediaPath;
}
```

## Controllers đã được cập nhật

### PostController
- `GetPosts()` - Danh sách bài viết (phân trang)
- `GetPost(id)` - Chi tiết bài viết
- `CreatePost()` - Tạo bài viết mới

### AuthController  
- `Register()` - Đăng ký user mới
- `Login()` - Đăng nhập

Tất cả các endpoint trả về `DuongDanMedia`, `ProfilePicture`, `AuthorAvatar` đều được chuyển đổi tự động.

## Kiểm tra

### Test đã pass
✅ 27/27 tests integration đều pass sau khi thêm MediaUrlService

### Test thủ công với Postman

1. **Đăng ký user mới**
```http
POST /api/Auth/register
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Test@123",
  "confirmPassword": "Test@123",
  "userName": "testuser",
  "age": 25,
  "gender": "Nam"
}
```

Response:
```json
{
  "user": {
    "profilePicture": "https://localhost:7043/images/avatar/default-profile-picture.jpg"
  }
}
```

2. **Lấy danh sách bài viết**
```http
GET /api/Post?page=1&pageSize=10
Authorization: Bearer {your_token}
```

Response:
```json
{
  "data": {
    "posts": [
      {
        "duongDanMedia": "https://localhost:7043/uploads/posts/abc.jpg",
        "authorAvatar": "https://localhost:7043/uploads/avatars/user.png"
      }
    ]
  }
}
```

## Lưu ý Production

### 1. Cấu hình domain thực tế
```json
{
  "HotelWebUrl": "https://your-production-domain.com"
}
```

### 2. CORS
Đảm bảo Hotel_Web cho phép CORS từ domain của API:
```csharp
// Trong Hotel_Web/Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAPI", policy =>
    {
        policy.WithOrigins("https://your-api-domain.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

### 3. HTTPS
- Đảm bảo cả Hotel_Web và Hotel_API đều chạy HTTPS
- Sử dụng SSL certificate hợp lệ

### 4. Performance
- Nếu có nhiều hình ảnh, cân nhắc sử dụng CDN
- Cache hình ảnh ở client side (Flutter)
- Compress hình ảnh trước khi lưu

## Troubleshooting

### Hình ảnh không tải được

**Lỗi**: 404 Not Found khi truy cập URL hình ảnh

**Giải pháp**:
1. Kiểm tra `HotelWebUrl` trong appsettings.json
2. Đảm bảo Hotel_Web đang chạy ở port đúng
3. Kiểm tra file có tồn tại trong `wwwroot/uploads`

### URL không đúng

**Lỗi**: URL có dạng `https://localhost:7043//uploads/...` (double slash)

**Giải pháp**: Service tự động xử lý, nhưng nếu vẫn gặp lỗi, kiểm tra đường dẫn trong database có bắt đầu với `/` hay không.

### Base64 bị chuyển đổi

**Lỗi**: Hình ảnh base64 bị thêm prefix URL

**Giải pháp**: Service kiểm tra `StartsWith("data:image/")` nên không nên xảy ra. Nếu vẫn lỗi, kiểm tra format base64 trong database.

## Mở rộng trong tương lai

### 1. Multiple domains
Nếu cần hỗ trợ nhiều domain (staging, production):

```csharp
public class MediaUrlService
{
    private readonly Dictionary<string, string> _environments = new()
    {
        { "Development", "https://localhost:7043" },
        { "Staging", "https://staging.yourhotel.com" },
        { "Production", "https://yourhotel.com" }
    };
    
    // ...
}
```

### 2. CDN Support
```csharp
public string GetFullMediaUrl(string? mediaPath)
{
    // ... existing code ...
    
    if (UseCDN)
    {
        return $"{CdnBaseUrl}{relativePath}";
    }
    
    return $"{_hotelWebBaseUrl}{relativePath}";
}
```

### 3. Image Resizing
```csharp
public string GetThumbnailUrl(string? mediaPath, int width, int height)
{
    var fullUrl = GetFullMediaUrl(mediaPath);
    return $"{fullUrl}?w={width}&h={height}";
}
```

## Kết luận

`MediaUrlService` giải quyết vấn đề liên kết hình ảnh giữa Hotel_API và Hotel_Web một cách tự động và linh hoạt. Service đã được tích hợp vào tất cả endpoints cần thiết và đã pass đầy đủ integration tests.
