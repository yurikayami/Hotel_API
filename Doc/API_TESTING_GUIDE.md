# 🚀 Hướng Dẫn Chạy và Test API - Hotel Web

## 📋 Yêu Cầu Hệ Thống

- .NET 9.0 SDK
- SQL Server (DESKTOP-YURI\SQLEXPRESS)
- Visual Studio 2022 hoặc VS Code

## 🔧 Cài Đặt & Chạy

### 1. Restore NuGet Packages

```powershell
cd "d:\Workspace\01 Project\Project Dev\Graduation project\Main Project\Hotel_API"
dotnet restore
```

### 2. Update Database (Nếu cần tạo database mới)

**Lưu ý**: Database `Hotel_Web` phải đã tồn tại trên SQL Server. Nếu chưa có, bạn cần:

```powershell
# Tạo migration (nếu chưa có)
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

### 3. Chạy API

```powershell
dotnet run
```

Hoặc nhấn `F5` trong Visual Studio.

API sẽ chạy tại: **https://localhost:7043** (hoặc port khác tùy cấu hình)

Swagger UI sẽ tự động mở tại: **https://localhost:7043**

---

## 📚 Test API với Swagger

### 1. Truy cập Swagger UI

Mở trình duyệt và truy cập: `https://localhost:7043`

Bạn sẽ thấy giao diện Swagger với tất cả các endpoints.

### 2. Test Authentication

#### **a. Đăng ký tài khoản mới**

1. Mở endpoint `POST /api/Auth/register`
2. Click "Try it out"
3. Nhập dữ liệu:

```json
{
  "userName": "testuser",
  "email": "test@example.com",
  "password": "Test@123",
  "confirmPassword": "Test@123",
  "age": 25,
  "gender": "Male"
}
```

4. Click "Execute"
5. **Lưu lại `token`** từ response để sử dụng cho các API khác

#### **b. Đăng nhập**

1. Mở endpoint `POST /api/Auth/login`
2. Click "Try it out"
3. Nhập:

```json
{
  "email": "test@example.com",
  "password": "Test@123"
}
```

4. Click "Execute"
5. **Copy token** từ response

#### **c. Authorize (Quan trọng!)**

Để test các API yêu cầu đăng nhập:

1. Click nút **"Authorize"** ở góc phải trên cùng của Swagger UI
2. Nhập: `Bearer <token_của_bạn>` (có chữ "Bearer " + dấu cách + token)
3. Click "Authorize"
4. Click "Close"

Bây giờ bạn đã được xác thực và có thể test các API cần đăng nhập!

---

### 3. Test Post API

#### **a. Lấy danh sách bài viết**

1. Endpoint: `GET /api/Post`
2. Click "Try it out"
3. Nhập parameters:
   - `page`: 1
   - `pageSize`: 10
4. Click "Execute"

#### **b. Tạo bài viết mới** (Cần Authorize)

1. **Đảm bảo đã Authorize** (xem bước 2c)
2. Endpoint: `POST /api/Post`
3. Click "Try it out"
4. Nhập:

```json
{
  "noiDung": "Đây là bài viết test từ Flutter app!",
  "loai": "general",
  "hashtags": "#test #flutter"
}
```

5. Click "Execute"
6. **Lưu lại `id`** của bài viết vừa tạo

#### **c. Xem chi tiết bài viết**

1. Endpoint: `GET /api/Post/{id}`
2. Click "Try it out"
3. Nhập `id` của bài viết (từ bước 3b)
4. Click "Execute"

#### **d. Like bài viết** (Cần Authorize)

1. Endpoint: `POST /api/Post/{id}/like`
2. Click "Try it out"
3. Nhập `id` của bài viết
4. Click "Execute"
5. Response sẽ trả về `isLiked: true` và `likeCount`
6. **Execute lần 2** để unlike (isLiked sẽ thành false)

#### **e. Lấy danh sách comment**

1. Endpoint: `GET /api/Post/{id}/comments`
2. Click "Try it out"
3. Nhập `id` của bài viết
4. Click "Execute"

#### **f. Thêm comment** (Cần Authorize)

1. Endpoint: `POST /api/Post/{id}/comments`
2. Click "Try it out"
3. Nhập `id` của bài viết
4. Nhập body:

```json
{
  "noiDung": "Đây là comment test!"
}
```

5. Click "Execute"

#### **g. Reply comment** (Cần Authorize)

1. Endpoint: `POST /api/Post/{id}/comments`
2. Click "Try it out"
3. Nhập `id` của bài viết
4. Nhập body:

```json
{
  "noiDung": "Đây là reply cho comment!",
  "parentCommentId": "guid-cua-comment-cha"
}
```

5. Click "Execute"

#### **h. Xóa bài viết** (Cần Authorize, chỉ người tạo)

1. Endpoint: `DELETE /api/Post/{id}`
2. Click "Try it out"
3. Nhập `id` của bài viết (phải là bài viết bạn tạo)
4. Click "Execute"

---

## 🔐 Cấu Trúc Response

### Success Response

```json
{
  "success": true,
  "message": "Thành công",
  "data": { ... },
  "errors": []
}
```

### Error Response

```json
{
  "success": false,
  "message": "Có lỗi xảy ra",
  "data": null,
  "errors": ["Chi tiết lỗi..."]
}
```

---

## 📱 Tích Hợp với Flutter

### 1. Setup HTTP Client

```dart
import 'package:http/http.dart' as http;
import 'dart:convert';

class ApiService {
  static const String baseUrl = 'https://localhost:7043/api';
  static String? _token;

  // Lưu token sau khi login
  static void setToken(String token) {
    _token = token;
  }

  // Headers với token
  static Map<String, String> get _headers => {
    'Content-Type': 'application/json',
    if (_token != null) 'Authorization': 'Bearer $_token',
  };
}
```

### 2. Đăng ký

```dart
Future<void> register() async {
  final response = await http.post(
    Uri.parse('$baseUrl/Auth/register'),
    headers: {'Content-Type': 'application/json'},
    body: jsonEncode({
      'userName': 'testuser',
      'email': 'test@example.com',
      'password': 'Test@123',
      'confirmPassword': 'Test@123',
      'age': 25,
      'gender': 'Male',
    }),
  );

  if (response.statusCode == 200) {
    final data = jsonDecode(response.body);
    if (data['success']) {
      ApiService.setToken(data['token']);
      print('Đăng ký thành công!');
    }
  }
}
```

### 3. Đăng nhập

```dart
Future<void> login() async {
  final response = await http.post(
    Uri.parse('$baseUrl/Auth/login'),
    headers: {'Content-Type': 'application/json'},
    body: jsonEncode({
      'email': 'test@example.com',
      'password': 'Test@123',
    }),
  );

  if (response.statusCode == 200) {
    final data = jsonDecode(response.body);
    if (data['success']) {
      ApiService.setToken(data['token']);
      print('Đăng nhập thành công!');
    }
  }
}
```

### 4. Lấy danh sách bài viết

```dart
Future<List<Post>> getPosts({int page = 1, int pageSize = 10}) async {
  final response = await http.get(
    Uri.parse('$baseUrl/Post?page=$page&pageSize=$pageSize'),
    headers: ApiService._headers,
  );

  if (response.statusCode == 200) {
    final data = jsonDecode(response.body);
    if (data['success']) {
      final posts = data['data']['posts'] as List;
      return posts.map((json) => Post.fromJson(json)).toList();
    }
  }
  return [];
}
```

### 5. Like bài viết

```dart
Future<void> likePost(String postId) async {
  final response = await http.post(
    Uri.parse('$baseUrl/Post/$postId/like'),
    headers: ApiService._headers,
  );

  if (response.statusCode == 200) {
    final data = jsonDecode(response.body);
    if (data['success']) {
      print('Like status: ${data['data']['isLiked']}');
      print('Like count: ${data['data']['likeCount']}');
    }
  }
}
```

### 6. Thêm comment

```dart
Future<void> addComment(String postId, String content) async {
  final response = await http.post(
    Uri.parse('$baseUrl/Post/$postId/comments'),
    headers: ApiService._headers,
    body: jsonEncode({
      'noiDung': content,
    }),
  );

  if (response.statusCode == 201) {
    final data = jsonDecode(response.body);
    if (data['success']) {
      print('Comment thành công!');
    }
  }
}
```

---

## ⚠️ Lưu Ý Quan Trọng

### 1. HTTPS & Certificate

Khi test với Flutter trên máy thật/emulator, bạn có thể gặp lỗi SSL Certificate. Fix bằng cách:

**Option 1: Cho phép certificate không hợp lệ (CHỈ DÙNG CHO DEV)**

```dart
import 'dart:io';

class MyHttpOverrides extends HttpOverrides {
  @override
  HttpClient createHttpClient(SecurityContext? context) {
    return super.createHttpClient(context)
      ..badCertificateCallback = (X509Certificate cert, String host, int port) => true;
  }
}

void main() {
  HttpOverrides.global = MyHttpOverrides();
  runApp(MyApp());
}
```

**Option 2: Dùng HTTP (không khuyến nghị)**

Trong `Program.cs`, comment dòng:
```csharp
// app.UseHttpsRedirection();
```

### 2. CORS

Nếu test từ browser/web app, đảm bảo CORS đã được cấu hình đúng trong `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutter", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ...

app.UseCors("AllowFlutter");
```

### 3. Connection String

Đảm bảo connection string trong `appsettings.json` đúng với SQL Server của bạn:

```json
"ConnectionStrings": {
  "HotelWebConnection": "Data Source=DESKTOP-YURI\\SQLEXPRESS;Initial Catalog=Hotel_Web;Integrated Security=True;TrustServerCertificate=True;"
}
```

### 4. JWT Token Expiry

Token có thời hạn 7 ngày (cấu hình trong `appsettings.json`). Sau khi hết hạn, user cần đăng nhập lại.

---

## 🐛 Troubleshooting

### Lỗi: "Cannot connect to database"

- Kiểm tra SQL Server đang chạy
- Kiểm tra connection string
- Chạy `dotnet ef database update`

### Lỗi: "401 Unauthorized"

- Đảm bảo đã Authorize trong Swagger
- Kiểm tra token còn hạn
- Token phải có prefix "Bearer "

### Lỗi: "403 Forbidden"

- Bạn không có quyền thực hiện hành động này
- Ví dụ: xóa bài viết của người khác

---

## 📞 Support

Nếu gặp vấn đề, kiểm tra:
1. Console logs của API
2. Response body từ Swagger
3. SQL Server logs

---

**Chúc bạn test API thành công! 🎉**
