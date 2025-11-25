# Hướng Dẫn Sử Dụng API Tìm Kiếm

## Tổng Quan

API Tìm Kiếm cung cấp chức năng tìm kiếm tổng quát và riêng từng loại dữ liệu:
- **Người dùng** (Users)
- **Bài đăng** (Posts)
- **Bài thuốc** (Medicines)
- **Món ăn** (Dishes)

Tất cả các API đều hỗ trợ tìm kiếm **không dấu** (ví dụ: "com ga" sẽ tìm được "cơm gà").

---

## 1. Tìm Kiếm Tổng Quát

### Endpoint
```
GET /api/search
```

### Parameters

| Parameter | Type | Required | Default | Mô Tả |
|-----------|------|----------|---------|-------|
| `q` | string | Yes | - | Query tìm kiếm (tối thiểu 2 ký tự) |
| `type` | string | No | "all" | Loại tìm kiếm: `all`, `users`, `posts`, `medicines`, `dishes` |
| `page` | integer | No | 1 | Trang hiện tại (pagination) |
| `limit` | integer | No | 20 | Số kết quả mỗi trang (max: 100) |

### Ví Dụ Request

```bash
# Tìm kiếm tất cả loại
GET /api/search?q=cơm&type=all&page=1&limit=20

# Tìm kiếm không dấu
GET /api/search?q=com+ga&page=1&limit=20

# Tìm kiếm chỉ bài thuốc
GET /api/search?q=cảm&type=medicines&page=1&limit=20

# Tìm kiếm chỉ người dùng
GET /api/search?q=nguyễn&type=users&page=1&limit=20
```

### Response Success (200 OK)

```json
{
  "success": true,
  "message": "Tìm kiếm thành công",
  "data": {
    "users": [
      {
        "id": "user-id-123",
        "name": "Nguyễn Văn A",
        "email": "user1@example.com",
        "avatar": "https://example.com/avatars/user_1.jpg",
        "displayName": "Người Dùng A"
      }
    ],
    "posts": [
      {
        "id": "post-id-123",
        "title": "Bài đăng về cơm gà...",
        "content": "Hướng dẫn nấu cơm gà ngon...",
        "image": "https://example.com/posts/post_101.jpg",
        "userId": "user-id-123",
        "userName": "Nguyễn Văn A",
        "createdAt": "2025-11-20T10:30:00Z",
        "viewCount": 150,
        "likeCount": 25
      }
    ],
    "medicines": [
      {
        "id": "medicine-id-123",
        "name": "Thuốc Cảm Hạ Sốt",
        "description": "Thuốc cảm hiệu quả, hạ sốt nhanh",
        "image": "https://example.com/medicines/med_201.jpg",
        "viewCount": 500,
        "likeCount": 50,
        "createdAt": "2025-11-01T08:00:00Z"
      }
    ],
    "dishes": [
      {
        "id": "dish-id-123",
        "name": "Cơm Gà Hainan",
        "description": "Cơm gà Hainan cổ điển",
        "image": "https://example.com/dishes/dish_301.jpg",
        "price": 65000,
        "category": "Cơm",
        "servings": 1,
        "viewCount": 200
      }
    ]
  },
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 100,
    "totalPages": 5
  }
}
```

### Response Error (400 Bad Request)

```json
{
  "success": false,
  "message": "Query phải từ 2 ký tự trở lên",
  "code": "INVALID_QUERY"
}
```

---

## 2. Tìm Kiếm Người Dùng

### Endpoint
```
GET /api/search/users
```

### Parameters

| Parameter | Type | Required | Default |
|-----------|------|----------|---------|
| `q` | string | Yes | - |
| `page` | integer | No | 1 |
| `limit` | integer | No | 20 |

### Ví Dụ Request

```bash
GET /api/search/users?q=nguyễn&page=1&limit=20
```

### Response Success (200 OK)

```json
{
  "success": true,
  "message": "Tìm kiếm người dùng thành công",
  "data": {
    "users": [
      {
        "id": "user-id-123",
        "name": "Nguyễn Văn A",
        "email": "user1@example.com",
        "avatar": "https://example.com/avatars/user_1.jpg",
        "displayName": "Người Dùng A"
      },
      {
        "id": "user-id-456",
        "name": "Nguyễn Thị B",
        "email": "user2@example.com",
        "avatar": "https://example.com/avatars/user_2.jpg",
        "displayName": "Người Dùng B"
      }
    ]
  },
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 50,
    "totalPages": 3
  }
}
```

---

## 3. Tìm Kiếm Bài Đăng

### Endpoint
```
GET /api/search/posts
```

### Parameters

| Parameter | Type | Required | Default |
|-----------|------|----------|---------|
| `q` | string | Yes | - |
| `page` | integer | No | 1 |
| `limit` | integer | No | 20 |

### Ví Dụ Request

```bash
GET /api/search/posts?q=nấu+ăn&page=1&limit=20
```

### Response Success (200 OK)

```json
{
  "success": true,
  "message": "Tìm kiếm bài đăng thành công",
  "data": {
    "posts": [
      {
        "id": "post-id-123",
        "title": "Bài đăng về nấu ăn...",
        "content": "Hướng dẫn nấu các món ăn ngon...",
        "image": "https://example.com/posts/post_101.jpg",
        "userId": "user-id-123",
        "userName": "Nguyễn Văn A",
        "createdAt": "2025-11-20T10:30:00Z",
        "viewCount": 150,
        "likeCount": 25
      }
    ]
  },
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 75,
    "totalPages": 4
  }
}
```

---

## 4. Tìm Kiếm Bài Thuốc

### Endpoint
```
GET /api/search/medicines
```

### Parameters

| Parameter | Type | Required | Default |
|-----------|------|----------|---------|
| `q` | string | Yes | - |
| `page` | integer | No | 1 |
| `limit` | integer | No | 20 |

### Ví Dụ Request - Hỗ Trợ Không Dấu

```bash
# Tìm với dấu
GET /api/search/medicines?q=cảm&page=1&limit=20

# Tìm không dấu (kết quả tương tự)
GET /api/search/medicines?q=cam&page=1&limit=20
```

### Response Success (200 OK)

```json
{
  "success": true,
  "message": "Tìm kiếm bài thuốc thành công",
  "data": {
    "medicines": [
      {
        "id": "medicine-id-123",
        "name": "Thuốc Cảm Hạ Sốt",
        "description": "Thuốc cảm hiệu quả, hạ sốt nhanh",
        "image": "https://example.com/medicines/med_201.jpg",
        "viewCount": 500,
        "likeCount": 50,
        "createdAt": "2025-11-01T08:00:00Z"
      }
    ]
  },
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 40,
    "totalPages": 2
  }
}
```

---

## 5. Tìm Kiếm Món Ăn

### Endpoint
```
GET /api/search/dishes
```

### Parameters

| Parameter | Type | Required | Default |
|-----------|------|----------|---------|
| `q` | string | Yes | - |
| `page` | integer | No | 1 |
| `limit` | integer | No | 20 |

### Ví Dụ Request - Hỗ Trợ Không Dấu

```bash
# Tìm với dấu
GET /api/search/dishes?q=cơm+gà&page=1&limit=20

# Tìm không dấu (kết quả tương tự)
GET /api/search/dishes?q=com+ga&page=1&limit=20
```

### Response Success (200 OK)

```json
{
  "success": true,
  "message": "Tìm kiếm món ăn thành công",
  "data": {
    "dishes": [
      {
        "id": "dish-id-123",
        "name": "Cơm Gà Hainan",
        "description": "Cơm gà Hainan cổ điển",
        "image": "https://example.com/dishes/dish_301.jpg",
        "price": 65000,
        "category": "Cơm",
        "servings": 1,
        "viewCount": 200
      }
    ]
  },
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 150,
    "totalPages": 8
  }
}
```

---

## 6. Gợi Ý Tìm Kiếm (Autocomplete)

### Endpoint
```
GET /api/search/suggestions
```

### Parameters

| Parameter | Type | Required | Default |
|-----------|------|----------|---------|
| `q` | string | Yes | - |
| `type` | string | No | "all" |
| `limit` | integer | No | 10 |

### Ví Dụ Request

```bash
# Gợi ý từ tất cả loại
GET /api/search/suggestions?q=cơ&limit=10

# Gợi ý chỉ từ món ăn
GET /api/search/suggestions?q=com&type=dishes&limit=10
```

### Response Success (200 OK)

```json
{
  "success": true,
  "message": "Lấy gợi ý thành công",
  "data": {
    "suggestions": [
      "cơm gà",
      "cơm chiên",
      "cơm tấm",
      "cơm cốc",
      "cơm dương",
      "cơm lạnh",
      "cơm trắng",
      "cơm xào",
      "cơm hàng",
      "cơm bình dân"
    ]
  }
}
```

---

## 7. Xử Lý Tìm Kiếm Không Dấu

### Cách Hoạt Động

SearchService có phương thức `NormalizeText()` chuyển đổi:
- "cơm gà" → "com ga"
- "nguyễn văn a" → "nguyen van a"  
- "thuốc cảm" → "thuoc cam"

### Ví Dụ Trong C#

```csharp
public string NormalizeText(string text)
{
    if (string.IsNullOrEmpty(text))
        return string.Empty;

    // Dictionary chuyển đổi ký tự có dấu thành không dấu
    var vietnameseChars = new Dictionary<string, string>
    {
        { "á", "a" }, { "à", "a" }, { "ả", "a" }, { "ã", "a" }, { "ạ", "a" },
        // ... (đầy đủ trong SearchService)
    };

    var result = text.ToLower();
    foreach (var kvp in vietnameseChars)
    {
        result = result.Replace(kvp.Key, kvp.Value);
    }

    return result.Trim();
}
```

---

## 8. Xử Lý Lỗi

### Các Lỗi Có Thể Gặp

| HTTP Status | Error Message | Mô Tả |
|-------------|------------------|-------|
| 400 | Query phải từ 2 ký tự trở lên | Query quá ngắn |
| 400 | Type không hợp lệ | Type parameter không hợp lệ |
| 400 | Page phải >= 1 | Page không hợp lệ |
| 400 | Limit phải từ 1-100 | Limit vượt quá giới hạn |
| 500 | Lỗi server: không thể thực hiện tìm kiếm | Lỗi server |

### Response Error

```json
{
  "success": false,
  "message": "Thông báo lỗi chi tiết",
  "code": "ERROR_CODE"
}
```

---

## 9. Hướng Dẫn Cài Đặt

### 1. Cập Nhật Database

Thực thi script migration SQL:
```bash
# Mở SQL Server Management Studio và chạy file SEARCH_MIGRATION.sql
```

### 2. Khởi Động API

```bash
# Trong thư mục project
dotnet run
```

### 3. Kiểm Tra Swagger

Truy cập: `http://localhost:5000/swagger/index.html` (hoặc port khác tùy cấu hình)

---

## 10. Cấu Hình

### SearchService được đăng ký trong Program.cs:

```csharp
builder.Services.AddScoped<SearchService>();
```

### Tùy chỉnh Limit Tối Đa

Thay đổi trong `SearchController` nếu cần:

```csharp
[Range(1, 100, ErrorMessage = "Limit phải từ 1-100")]
public int Limit { get; set; } = 20;
```

---

## 11. Ví Dụ Flutter Integration

### Sử Dụng SearchAnchor

```dart
import 'package:flutter/material.dart';

class SearchExample extends StatefulWidget {
  @override
  State<SearchExample> createState() => _SearchExampleState();
}

class _SearchExampleState extends State<SearchExample> {
  final SearchController _controller = SearchController();

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: SearchAnchor(
          builder: (BuildContext context, SearchController controller) {
            return SearchBar(
              controller: controller,
              hintText: 'Tìm kiếm...',
              onChanged: (_) async {
                final query = _controller.text;
                if (query.length >= 2) {
                  // Gọi API suggestions
                  final suggestions = await http.get(
                    Uri.parse(
                      'http://your-api.com/api/search/suggestions?q=$query&limit=10'
                    ),
                  );
                  // Xử lý suggestions
                }
              },
              onSubmitted: (String value) async {
                // Gọi API search
                final results = await http.get(
                  Uri.parse(
                    'http://your-api.com/api/search?q=$value&type=all&page=1&limit=20'
                  ),
                );
                // Xử lý results
              },
            );
          },
          suggestionsBuilder: (BuildContext context, SearchController controller) {
            return <Widget>[
              // Hiển thị suggestions
            ];
          },
        ),
      ),
      body: Center(
        child: Text('Search Results'),
      ),
    );
  }
}
```

---

## 12. Performance Tips

1. **Pagination**: Luôn sử dụng `page` và `limit` để tránh tải quá nhiều dữ liệu
2. **Minimum Length**: Query tối thiểu 2 ký tự để giảm tải cho server
3. **Caching**: Xem xét cache kết quả tìm kiếm phổ biến
4. **Rate Limiting**: Recommend giới hạn 100 requests/phút per IP
5. **Indexing**: Database index trên cột `TenNormalized`, `NoiDungNormalized` để tăng tốc độ tìm kiếm

---

## 13. Troubleshooting

### Vấn đề: Tìm kiếm không tìm được kết quả

**Giải pháp**:
- Kiểm tra query có chứa ký tự đặc biệt
- Thử tìm kiếm không dấu
- Kiểm tra xem dữ liệu có tồn tại trong database

### Vấn đề: Tìm kiếm rất chậm

**Giải pháp**:
- Kiểm tra index database
- Thực thi SEARCH_MIGRATION.sql để tạo index
- Limit số lượng kết quả

### Vấn đề: Lỗi 500 Server Error

**Giải pháp**:
- Kiểm tra database connection string
- Xem logs của application
- Đảm bảo SearchService được đăng ký trong DI container

---

## 14. Liên Hệ & Hỗ Trợ

Nếu gặp vấn đề, vui lòng kiểm tra:
1. Database connection
2. API logs
3. Swagger documentation tại `/swagger/index.html`
