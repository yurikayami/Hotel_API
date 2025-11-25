# Chức Năng Tìm Kiếm - Tóm Tắt Phát Triển

## Giới Thiệu

Đã phát triển chức năng tìm kiếm tổng quát cho API Hotel, hỗ trợ tìm kiếm trên 4 loại dữ liệu chính:
- 👤 **Người dùng** (Users)
- 📝 **Bài đăng** (Posts)
- 💊 **Bài thuốc** (Medicines)
- 🍜 **Món ăn** (Dishes)

Tất cả các endpoint đều **hỗ trợ tìm kiếm không dấu**, ví dụ:
- `com ga` → tìm được `cơm gà`
- `thuoc cam` → tìm được `thuốc cảm`
- `nguyen` → tìm được `Nguyễn`

---

## 📁 File Được Tạo/Cập Nhật

### Controllers
```
Controllers/SearchController.cs - NEW
```
- 5 endpoints chính (1 tổng quát + 4 riêng từng loại + 1 suggestions)
- Validation và error handling đầy đủ
- Swagger documentation

### Services
```
Services/SearchService.cs - NEW
```
- Logic tìm kiếm tổng quát
- Normalize text không dấu (Vietnamese character mapping)
- Async database queries sử dụng Entity Framework
- Xử lý lỗi toàn diện

### ViewModels
```
Models/ViewModels/SearchViewModels.cs - NEW
```
- `SearchRequestDto` - Request model
- `UserSearchResultDto` - User results
- `PostSearchResultDto` - Post results
- `MedicineSearchResultDto` - Medicine results
- `DishSearchResultDto` - Dish results
- `GeneralSearchResponseDto` - Response wrapper
- `SuggestionsResponseDto` - Autocomplete response
- `ErrorResponseDto` - Error response

### Configuration
```
Program.cs - UPDATED
```
- Đăng ký `SearchService` trong DI container

### Documentation
```
Doc/SEARCH_API_GUIDE.md - NEW
Doc/SEARCH_MIGRATION.sql - NEW
```

### Testing
```
API_SEARCH_TESTS.http - NEW
```
- 20 test cases sẵn sàng sử dụng Rest Client

---

## 🔌 API Endpoints

### 1. Tìm Kiếm Tổng Quát
```http
GET /api/search?q=cơm&type=all&page=1&limit=20
```

### 2. Tìm Kiếm Người Dùng
```http
GET /api/search/users?q=nguyễn&page=1&limit=20
```

### 3. Tìm Kiếm Bài Đăng
```http
GET /api/search/posts?q=nấu+ăn&page=1&limit=20
```

### 4. Tìm Kiếm Bài Thuốc
```http
GET /api/search/medicines?q=cảm&page=1&limit=20
```

### 5. Tìm Kiếm Món Ăn
```http
GET /api/search/dishes?q=cơm+gà&page=1&limit=20
```

### 6. Gợi Ý (Autocomplete)
```http
GET /api/search/suggestions?q=cơ&type=all&limit=10
```

---

## ✨ Tính Năng Chính

### ✅ Tìm Kiếm Không Dấu
- Chuyển đổi tất cả ký tự có dấu tiếng Việt
- Ví dụ: `cơm` ↔ `com`, `nguyễn` ↔ `nguyen`

### ✅ Pagination
- Hỗ trợ trang (page) và số lượng kết quả (limit)
- Limit max: 100, mặc định: 20

### ✅ Filtering
- Tìm kiếm tất cả loại hoặc riêng từng loại
- Parameters: `all`, `users`, `posts`, `medicines`, `dishes`

### ✅ Validation
- Query tối thiểu 2 ký tự
- Kiểm tra type, page, limit hợp lệ
- Sanitize input để tránh SQL injection (sử dụng EF.Functions)

### ✅ Error Handling
- Responses rõ ràng với error codes
- Logging đầy đủ

### ✅ Performance
- Sử dụng EF.Functions.Like() cho tìm kiếm hiệu quả
- Database index trên cột normalized (có thể thêm sau)

---

## 🚀 Cách Sử Dụng

### 1. Thực Thi Migration SQL
```bash
# Mở SQL Server Management Studio
# Chạy: Doc/SEARCH_MIGRATION.sql
# (Tạo index để tăng tốc độ tìm kiếm)
```

### 2. Khởi Động API
```bash
dotnet run
```

### 3. Kiểm Tra Swagger
Truy cập: `http://localhost:5000/swagger`

### 4. Test API
Sử dụng file `API_SEARCH_TESTS.http` trong VS Code Rest Client hoặc Postman

---

## 📝 Request/Response Examples

### Request
```http
GET /api/search?q=com+ga&type=all&page=1&limit=20
```

### Response Success (200)
```json
{
  "success": true,
  "message": "Tìm kiếm thành công",
  "data": {
    "users": [...],
    "posts": [...],
    "medicines": [...],
    "dishes": [...]
  },
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 100,
    "totalPages": 5
  }
}
```

### Response Error (400)
```json
{
  "success": false,
  "message": "Query phải từ 2 ký tự trở lên",
  "code": "INVALID_QUERY"
}
```

---

## 🔍 Chi Tiết Implement

### Normalize Text Function
```csharp
public string NormalizeText(string text)
{
    // Chuyển đổi: "cơm gà" → "com ga"
    // Hỗ trợ đầy đủ ký tự tiếng Việt có dấu
}
```

### Search Methods
- `SearchGeneralAsync()` - Tìm kiếm tổng quát
- `SearchUsersAsync()` - Tìm kiếm người dùng
- `SearchPostsAsync()` - Tìm kiếm bài đăng
- `SearchMedicinesAsync()` - Tìm kiếm bài thuốc
- `SearchDishesAsync()` - Tìm kiếm món ăn
- `GetSuggestionsAsync()` - Lấy gợi ý

---

## 📊 Database Changes

### Cột Được Thêm (Optional)
- `MonAn.TenNormalized` - Tên không dấu
- `BaiDang.NoiDungNormalized` - Nội dung không dấu
- `BaiThuoc.TenNormalized` - Tên không dấu
- `AspNetUsers.displayNameNormalized` - Tên hiển thị không dấu

### Index Được Tạo
```sql
CREATE INDEX idx_MonAn_TenNormalized ON MonAn(TenNormalized);
CREATE INDEX idx_BaiDang_NoiDungNormalized ON BaiDang(NoiDungNormalized);
CREATE INDEX idx_BaiThuoc_TenNormalized ON BaiThuoc(TenNormalized);
CREATE INDEX idx_AspNetUsers_displayNameNormalized ON AspNetUsers(displayNameNormalized);
```

---

## 🛠️ Cấu Hình

### Program.cs
```csharp
// Đã thêm
builder.Services.AddScoped<SearchService>();
```

### appsettings.json
Không cần cấu hình thêm, sử dụng database connection hiện tại.

---

## 🧪 Testing

### Sử dụng Rest Client (VS Code)
```bash
# File: API_SEARCH_TESTS.http
# Có 20 test cases sẵn sàng
```

### Sử dụng cURL
```bash
curl "http://localhost:5000/api/search?q=cơm&type=all&page=1&limit=20"
```

### Sử dụng Postman
Import các test cases từ `API_SEARCH_TESTS.http`

---

## 📋 Checklist Phát Triển

- ✅ Tạo ViewModels (SearchViewModels.cs)
- ✅ Tạo Service (SearchService.cs)
  - ✅ Normalize text không dấu
  - ✅ Tìm kiếm tổng quát
  - ✅ Tìm kiếm riêng từng loại
  - ✅ Gợi ý tìm kiếm
- ✅ Tạo Controller (SearchController.cs)
  - ✅ 6 endpoints chính
  - ✅ Validation đầy đủ
  - ✅ Error handling
- ✅ Cập nhật Program.cs (DI registration)
- ✅ Tạo Migration SQL
- ✅ Tạo Documentation
- ✅ Tạo Test Cases

---

## 📚 Tài Liệu Liên Quan

- **Hướng Dẫn Chi Tiết**: `Doc/SEARCH_API_GUIDE.md`
- **Migration Database**: `Doc/SEARCH_MIGRATION.sql`
- **Test Examples**: `API_SEARCH_TESTS.http`
- **API Documentation**: `API_SEARCH_DOCUMENTATION.md` (gốc)

---

## 🤝 Tích Hợp Flutter

### SearchAnchor Widget
```dart
SearchAnchor(
  builder: (context, controller) {
    return SearchBar(
      controller: controller,
      onChanged: (_) {
        // Gọi /api/search/suggestions
      },
      onSubmitted: (value) {
        // Gọi /api/search
      },
    );
  },
  suggestionsBuilder: (context, controller) {
    // Hiển thị gợi ý từ API
  },
)
```

---

## ⚡ Performance Recommendations

1. **Pagination**: Luôn sử dụng page và limit
2. **Query Length**: Tối thiểu 2 ký tự
3. **Caching**: Cache kết quả phổ biến
4. **Rate Limiting**: ~100 requests/phút per IP
5. **Database Index**: Chạy SEARCH_MIGRATION.sql
6. **Connection Pooling**: Sử dụng default từ EF

---

## 🐛 Troubleshooting

| Vấn Đề | Giải Pháp |
|--------|----------|
| 404 - Endpoint không tìm thấy | Đảm bảo SearchController đang trong Controllers folder |
| 500 - Server Error | Kiểm tra database connection, xem logs |
| Tìm kiếm không tìm được | Thử query không dấu, kiểm tra dữ liệu có tồn tại |
| Tìm kiếm chậm | Chạy migration SQL để tạo index |

---

## 📞 Hỗ Trợ

Tham khảo `Doc/SEARCH_API_GUIDE.md` để có hướng dẫn chi tiết hơn.

---

**Ngày tạo**: 21/11/2025  
**Version**: 1.0  
**Status**: ✅ Sẵn sàng sử dụng
