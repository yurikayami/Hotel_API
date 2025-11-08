# 📘 Tài Liệu API - Hotel Web Backend

> **Mục đích**: Tài liệu này mô tả các API endpoints hiện có trong hệ thống, được thiết kế để tích hợp với ứng dụng Flutter.

---

## 📋 Mục Lục

1. [Thông Tin Chung](#thông-tin-chung)
2. [Authentication & User Management](#1-authentication--user-management)
3. [Social Feed API](#2-social-feed-api-postapi)
4. [Food Order API](#3-food-order-api-orderfoodapi)
5. [Bài Thuốc API](#4-bài-thuốc-api-baithuocapi)
6. [Food Analysis API](#5-food-analysis-api-foodanalysis)
7. [Health Profile API](#6-health-profile-api-healthprofile)
8. [Models & DTOs](#7-models--dtos-reference)
9. [Error Handling](#8-error-handling)
10. [Best Practices](#9-best-practices)

---

## Thông Tin Chung

### Base URL
```
https://your-domain.com/api
```

### Authentication
Hệ thống sử dụng **ASP.NET Identity** với Cookie-based authentication. 

**Lưu ý**: 
- Các endpoint có `[Authorize]` yêu cầu đăng nhập
- Các endpoint không có attribute có thể truy cập anonymous
- User ID được lấy từ `User.Identity` sau khi đăng nhập

### Response Format
Tất cả response đều trả về JSON format:

```json
{
  "success": true,
  "data": {...},
  "message": "Optional message"
}
```

### Pagination
Các endpoint hỗ trợ phân trang thường có format:
- `page`: Số trang (mặc định 1)
- `pageSize`: Số items/trang (mặc định 10)

---

## 1. Authentication & User Management

### 1.1 Đăng Ký (Register)

**Endpoint**: `POST /Account/Register`

**Description**: Tạo tài khoản người dùng mới

**Request Body**:
```json
{
  "userName": "string",
  "email": "string",
  "password": "string",
  "age": 25,
  "gender": "Male/Female"
}
```

**Response Success** (302 Redirect):
```
Redirect to: /TestLayout/Index
```

**Response Error** (400):
```json
{
  "errors": [
    "Password must be at least 6 characters",
    "Email is already taken"
  ]
}
```

**Validation Rules**:
- Password: Tối thiểu 6 ký tự, có chữ số
- Email: Phải unique
- UserName: Bắt buộc

---

### 1.2 Đăng Nhập Google

**Endpoint**: `GET /Account/GoogleLogin?returnUrl={url}`

**Description**: Khởi động OAuth flow với Google

**Query Parameters**:
- `returnUrl` (optional): URL để redirect sau khi login thành công

**Flow**:
1. User click "Login with Google"
2. Redirect đến Google OAuth consent screen
3. Google callback về `/Account/GoogleResponse`
4. Tự động tạo user nếu chưa tồn tại
5. Gán role "User"
6. Redirect về returnUrl hoặc `/HomePage/Index`

---

### 1.3 Đăng Xuất

**Endpoint**: `POST /Account/Logout`

**Description**: Đăng xuất và cập nhật trạng thái offline

**Side Effects**:
- Set `dang_online = false`
- Set `trang_thai = 0`
- Update `lan_hoat_dong_cuoi = DateTime.UtcNow`
- Clear authentication cookie

**Response**: Redirect to `/Home/Index`

---

## 2. Social Feed API (`PostAPI`)

Base route: `/api/PostAPI`

### 2.1 Lấy Home Feed

**Endpoint**: `GET /api/PostAPI/feed`

**Description**: Lấy danh sách bài viết cho trang chủ với thuật toán mix content

**Query Parameters**:
```
page=1         // Số trang (default: 1)
pageSize=10    // Số items/trang (default: 10)
```

**Response Success** (200):
```json
[
  {
    "id": "guid",
    "type": "Post",  // hoặc "BaiThuoc"
    "content": "Nội dung bài viết...",
    "imageUrl": "/uploads/image.jpg",
    "ngayDang": "2025-01-15T10:30:00",
    "soBinhLuan": 10,
    "soChiaSe": 5,
    "luotThich": 25,
    "isLiked": true,
    "authorId": "user-id",
    "authorName": "John Doe",
    "avartar": "/images/avatar.jpg"
  }
]
```

**Content Mixing Algorithm**:
Feed được mix theo tỷ lệ:
- 2 Friend Posts
- 2 Friend BaiThuoc
- 3 Top BaiThuoc (by views)
- 2 Random Posts
- 1 Random BaiThuoc

**Authentication**: Optional (nếu không đăng nhập, chỉ hiện random content)

---

### 2.2 Xem Chi Tiết Bài Viết

**Endpoint**: `GET /api/PostAPI/detail`

**Description**: Lấy chi tiết một bài viết hoặc bài thuốc

**Query Parameters**:
```
id=guid           // ID của bài viết (required)
type=Post         // "Post" hoặc "BaiThuoc" (optional, tự detect nếu không có)
```

**Response Success - Post** (200):
```json
{
  "id": "guid",
  "content": "Nội dung bài viết...",
  "imageUrl": "/uploads/image.jpg",
  "ngayDang": "2025-01-15T10:30:00",
  "soBinhLuan": 10,
  "soChiaSe": 5,
  "luotThich": 25,
  "isLiked": true,
  "authorId": "user-id",
  "authorName": "John Doe",
  "avartar": "/images/avatar.jpg"
}
```

**Response Success - BaiThuoc** (200):
```json
{
  "id": "guid",
  "tieuDe": "Tên bài thuốc",
  "moTa": "Mô tả chi tiết...",
  "imageUrl": "/uploads/baithuoc.jpg",
  "ngayTao": "2025-01-15T10:30:00",
  "soLuotThich": 15,
  "isLiked": false,
  "authorId": "user-id",
  "authorName": "Doctor Smith",
  "avartar": "/images/doctor.jpg",
  "nguyenLieu": null,    // TODO: Chưa implement
  "huongDan": null,      // TODO: Chưa implement
  "congDung": null       // TODO: Chưa implement
}
```

**Response Error** (404):
```json
{
  "message": "Không tìm thấy bài viết."
}
```

---

## 3. Food Order API (`OrderFoodAPI`)

Base route: `/api/OrderFoodAPI`

### 3.1 Lấy Giỏ Hàng

**Endpoint**: `GET /api/OrderFoodAPI/GetCartItem`

**Description**: Lấy danh sách món ăn trong giỏ hàng của user hiện tại

**Authentication**: Required

**Response Success** (200):
```json
{
  "success": true,
  "cartItems": [
    {
      "id": "guid",
      "monAnId": "guid",
      "tenMonAn": "Phở Bò",
      "soLuong": 2,
      "donGia": 50000,
      "thanhTien": 100000,
      "imageUrl": "/uploads/food/pho.jpg"
    }
  ]
}
```

**Response Error - Not Logged In** (200):
```json
{
  "success": false,
  "message": "Vui lòng đăng nhập để xem giỏ hàng.",
  "cartItems": []
}
```

**Response Error - Exception** (200):
```json
{
  "success": false,
  "message": "Lỗi: {exception message}",
  "cartItems": []
}
```

---

### 3.2 Cập Nhật Số Lượng

**Endpoint**: `POST /api/OrderFoodAPI/UpdateQuantities`

**Description**: Cập nhật số lượng món ăn trong giỏ hàng

**Request Body**:
```json
{
  "chiTietId": "guid",
  "soLuong": 3
}
```

**Validation**:
- `soLuong` >= 1

**Response Success** (200):
```json
{
  "success": true,
  "newTotal": 150000
}
```

**Response Error** (400):
```json
"Dữ liệu không hợp lệ"
```

**Response Error** (404):
```json
"Không tìm thấy món trong giỏ hàng"
```

**Business Logic**:
- Tự động tính lại `ThanhTien = MonAn.Gia * soLuong`
- Update `GioHang.NgayCapNhat = DateTime.Now`

---

## 4. Bài Thuốc API (`BaiThuocAPI`)

Base route: `/api/BaiThuocAPI`

### 4.1 Tạo Bài Thuốc

**Endpoint**: `POST /api/BaiThuocAPI/create`

**Description**: Tạo bài thuốc mới với ảnh

**Content-Type**: `multipart/form-data`

**Request Body (Form Data)**:
```
Ten: "Bài thuốc chữa cảm"           (required)
MoTa: "Mô tả chi tiết..."           (optional)
HuongDanSuDung: "Hướng dẫn..."      (optional)
NguoiDungId: "user-id"              (optional)
Image: <file>                        (optional)
NgayTao: "2025-01-15T10:30:00"      (optional, default: DateTime.Now)
```

**Validation**:
- `Ten` không được để trống

**Response Success** (200):
```json
{
  "message": "Tạo thành công",
  "data": {
    "id": "guid",
    "ten": "Bài thuốc chữa cảm",
    "moTa": "Mô tả chi tiết...",
    "huongDanSuDung": "Hướng dẫn...",
    "nguoiDungId": "user-id",
    "ngayTao": "2025-01-15T10:30:00",
    "image": "/uploads/baithuoc/abc123.jpg",
    "soLuotThich": null,
    "soLuotXem": null,
    "trangThai": null
  }
}
```

**Response Error** (400):
```json
{
  "message": "Tên không được để trống"
}
```

**File Upload**:
- Thư mục: `wwwroot/uploads/baithuoc/`
- Filename format: `{Guid}{extension}`
- Tự động tạo folder nếu chưa tồn tại

---

## 5. Food Analysis API (`FoodAnalysis`)

Base route: `/api/FoodAnalysis`

### 5.1 Phân Tích Ảnh Món Ăn

**Endpoint**: `POST /api/FoodAnalysis/analyze`

**Description**: Upload ảnh món ăn để AI phân tích dinh dưỡng và đưa ra lời khuyên

**Content-Type**: `multipart/form-data`

**Request Body (Form Data)**:
```
image: <file>              (required, image file)
userId: "user-id"          (required)
mealType: "breakfast"      (optional: breakfast/lunch/dinner/snack)
```

**Validation**:
- `userId` không được empty
- `image` phải là file image (content-type: image/*)

**Response Success** (200):
```json
{
  "success": true,
  "imageUrl": "https://domain.com/uploads/abc123.jpg",
  "prediction": {
    "predicted_label": "Phở Bò",
    "confidence": 0.95,
    "nutrition": {
      "calories": 450,
      "protein": 25,
      "carbs": 60,
      "fat": 12,
      "fiber": 3,
      "mealType": "lunch"
    }
  },
  "planAdvice": {
    "isWithinCalorieLimit": true,
    "remainingCalories": 550,
    "message": "Bữa ăn này phù hợp với phác đồ của bạn.",
    "recommendations": [
      "Nên uống thêm nước",
      "Tránh ăn thêm đồ chiên rán"
    ]
  }
}
```

**Response Error** (400):
```json
{
  "error": "User ID is required"
}
// hoặc
{
  "error": "Invalid image file."
}
// hoặc
{
  "error": "Không tìm thấy phác đồ của người dùng."
}
```

**External Dependencies**:
- Gọi Python API để phân tích ảnh
- Gọi `NutritionService.GetMealAdviceAsync()` để đưa ra lời khuyên

**Business Logic**:
1. Upload và lưu ảnh vật lý tại `wwwroot/uploads/`
2. Gọi Python API để detect món ăn và dinh dưỡng
3. Lấy phác đồ gần nhất của user
4. So sánh với phác đồ và đưa ra lời khuyên
5. Lưu lịch sử phân tích (TODO: implement lưu DB)

---

## 6. Health Profile API (`HealthProfile`)

Base route: `/api/HealthProfile`

### 6.1 Lấy Hồ Sơ Sức Khỏe

**Endpoint**: `GET /api/HealthProfile`

**Description**: Lấy thông tin hồ sơ sức khỏe đầy đủ của user

**Authentication**: Required

**Response Success** (200):
```json
{
  "id": "profile-id",
  "userId": "user-id",
  "fullName": "Nguyễn Văn A",
  "age": 25,
  "gender": "Male",
  "dateOfBirth": "1999-01-15",
  "bloodType": "O+",
  "emergencyContactName": "Nguyễn Thị B",
  "emergencyContactPhone": "0123456789",
  "hasDiabetes": false,
  "hasHypertension": false,
  "hasAsthma": false,
  "hasHeartDisease": false,
  "otherDiseases": "Dị ứng phấn hoa",
  "drugAllergies": "Penicillin",
  "foodAllergies": "Hải sản",
  "hasLatexAllergy": false,
  "currentMedicationsJson": "[{\"name\":\"Aspirin\",\"dosage\":\"100mg\"}]",
  "insuranceNumber": "BH123456",
  "insuranceProvider": "Bảo Việt",
  "emergencyNotes": "Cần chú ý...",
  "weight": 70.5,
  "height": 175,
  "activityLevel": "Moderate",
  "createdAt": "2025-01-01T00:00:00",
  "updatedAt": "2025-01-15T10:30:00"
}
```

**Response Error** (500):
```json
{
  "error": "Không thể lấy hồ sơ sức khỏe",
  "details": "Exception message"
}
```

---

### 6.2 Kiểm Tra Độ Hoàn Thiện

**Endpoint**: `GET /api/HealthProfile/completion`

**Description**: Đánh giá mức độ hoàn thiện của hồ sơ sức khỏe

**Authentication**: Required

**Response Success** (200):
```json
{
  "completionPercentage": 75,
  "missingFields": [
    "BloodType",
    "EmergencyContactPhone"
  ],
  "recommendations": [
    "Bổ sung nhóm máu để phòng cấp cứu",
    "Thêm số điện thoại người thân"
  ]
}
```

---

### 6.3 Cập Nhật Thông Tin Cá Nhân

**Endpoint**: `POST /api/HealthProfile/personal-info`

**Description**: Cập nhật thông tin cơ bản (tuổi, giới tính, chiều cao, cân nặng...)

**Authentication**: Required

**Request Body**:
```json
{
  "fullName": "Nguyễn Văn A",
  "age": 25,
  "gender": "Male",
  "dateOfBirth": "1999-01-15",
  "bloodType": "O+",
  "weight": 70.5,
  "height": 175,
  "activityLevel": "Moderate"
}
```

**Response Success** (200):
```json
{
  "id": "profile-id",
  "userId": "user-id",
  ...
  // Full health profile object
}
```

**Response Error** (401):
```json
{
  "error": "Bạn cần đăng nhập để cập nhật"
}
```

---

### 6.4 Cập Nhật Bệnh Lý Mãn Tính

**Endpoint**: `POST /api/HealthProfile/chronic-conditions`

**Description**: Cập nhật thông tin bệnh mãn tính

**Authentication**: Required

**Request Body**:
```json
{
  "hasDiabetes": true,
  "hasHypertension": false,
  "hasAsthma": false,
  "hasHeartDisease": false,
  "otherDiseases": "Viêm gan B"
}
```

**Response Success** (200):
```json
{
  "id": "profile-id",
  ...
  // Full health profile object with updated chronic conditions
}
```

---

## 7. Models & DTOs Reference

### 7.1 Core Models

#### ApplicationUser
```csharp
{
  "id": "string",
  "userName": "string",
  "email": "string",
  "gioi_tinh": "string",
  "tuoi": "int?",
  "profilePicture": "string",
  "isFacebookLinked": "bool?",
  "isGoogleLinked": "bool?",
  "dang_online": "bool?",
  "googleProfilePicture": "string",
  "facebookProfilePicture": "string",
  "trang_thai": "int?",
  "lan_hoat_dong_cuoi": "DateTime?",
  "displayName": "string",
  "avatarUrl": "string",
  "kinh_nghiem": "int?",  // Cho bác sĩ
  "chuyenKhoaId": "Guid?"
}
```

#### BaiDang (Post)
```csharp
{
  "id": "Guid",
  "nguoiDungId": "string",
  "noiDung": "string",
  "loai": "string",
  "duongDanMedia": "string",
  "ngayDang": "DateTime?",
  "id_MonAn": "Guid?",
  "luotThich": "int?",
  "soBinhLuan": "int?",
  "nguoiDang": "string",
  "daDuyet": "bool?",
  "so_chia_se": "int",
  "hashtags": "string",
  "keywords": "string"
}
```

#### BaiThuoc
```csharp
{
  "id": "Guid",
  "ten": "string",
  "moTa": "string",
  "huongDanSuDung": "string",
  "nguoiDungId": "string",
  "ngayTao": "DateTime",
  "image": "string",
  "soLuotThich": "int?",
  "soLuotXem": "int?",
  "trangThai": "int?"
}
```

#### MonAn (Dish)
```csharp
{
  "id": "Guid",
  "ten": "string",
  "moTa": "string",
  "cachCheBien": "string",
  "loai": "string",  // Max 50 chars
  "ngayTao": "DateTime?",
  "image": "string",
  "gia": "decimal(10,2)?",
  "soNguoi": "int?",
  "luotXem": "int"
}
```

#### GioHang (Cart)
```csharp
{
  "id": "Guid",
  "nguoiDungId": "string",
  "ngayTao": "DateTime",
  "ngayCapNhat": "DateTime",
  "trangThai": "int?",
  "chiTiets": [
    {
      "id": "Guid",
      "gioHangId": "Guid",
      "monAnId": "Guid",
      "soLuong": "int",
      "thanhTien": "decimal"
    }
  ]
}
```

#### HealthProfile
```csharp
{
  "id": "string",
  "userId": "string",
  "age": "int",
  "gender": "string",
  "fullName": "string",
  "dateOfBirth": "DateTime?",
  "bloodType": "string",
  "emergencyContactName": "string",
  "emergencyContactPhone": "string",
  "hasDiabetes": "bool",
  "hasHypertension": "bool",
  "hasAsthma": "bool",
  "hasHeartDisease": "bool",
  "otherDiseases": "string",
  "drugAllergies": "string",
  "foodAllergies": "string",
  "hasLatexAllergy": "bool",
  "currentMedicationsJson": "string",
  "insuranceNumber": "string",
  "insuranceProvider": "string",
  "emergencyNotes": "string",
  "weight": "double?",
  "height": "double?",
  "activityLevel": "string",
  "createdAt": "DateTime?",
  "updatedAt": "DateTime?"
}
```

---

## 8. Error Handling

### Standard Error Response Format

```json
{
  "success": false,
  "error": "Error message",
  "details": "Detailed exception message (only in development)",
  "statusCode": 400
}
```

### Common HTTP Status Codes

| Code | Meaning | Usage |
|------|---------|-------|
| 200 | OK | Request thành công |
| 400 | Bad Request | Dữ liệu không hợp lệ, thiếu required fields |
| 401 | Unauthorized | Chưa đăng nhập hoặc token invalid |
| 403 | Forbidden | Không có quyền truy cập |
| 404 | Not Found | Resource không tồn tại |
| 500 | Internal Server Error | Lỗi server |

### Error Examples

**Validation Error**:
```json
{
  "errors": {
    "Email": ["Email is required"],
    "Password": ["Password must be at least 6 characters"]
  }
}
```

**Authorization Error**:
```json
{
  "error": "Bạn cần đăng nhập để thực hiện thao tác này"
}
```

**Not Found Error**:
```json
{
  "message": "Không tìm thấy bài viết."
}
```

---

## 9. Best Practices

### 9.1 Cho Flutter Developer

#### Authentication Flow
```dart
// 1. Login với Google OAuth
// Redirect user đến: https://domain.com/Account/GoogleLogin
// Sau khi callback, lưu cookie authentication

// 2. Gọi API với authenticated request
final response = await http.get(
  Uri.parse('https://domain.com/api/PostAPI/feed'),
  headers: {
    'Cookie': savedCookie,  // Cookie từ login
  },
);
```

#### Pagination Best Practice
```dart
// Implement infinite scroll
int currentPage = 1;
final pageSize = 10;

Future<void> loadMorePosts() async {
  final response = await http.get(
    Uri.parse('https://domain.com/api/PostAPI/feed?page=$currentPage&pageSize=$pageSize'),
  );
  
  if (response.statusCode == 200) {
    final posts = jsonDecode(response.body);
    if (posts.isNotEmpty) {
      currentPage++;
      // Add to your list
    }
  }
}
```

#### Image Upload
```dart
// Upload với multipart/form-data
var request = http.MultipartRequest(
  'POST',
  Uri.parse('https://domain.com/api/BaiThuocAPI/create'),
);

request.fields['Ten'] = 'Bài thuốc mới';
request.fields['MoTa'] = 'Mô tả...';
request.files.add(
  await http.MultipartFile.fromPath('Image', imagePath),
);

var response = await request.send();
```

#### Error Handling
```dart
try {
  final response = await http.get(url);
  
  if (response.statusCode == 200) {
    final data = jsonDecode(response.body);
    return data;
  } else if (response.statusCode == 401) {
    // Redirect to login
    navigateToLogin();
  } else {
    throw Exception('Error: ${response.body}');
  }
} catch (e) {
  print('Network error: $e');
  showErrorDialog('Không thể kết nối đến server');
}
```

---

### 9.2 Caching Strategy

**Nên cache**:
- User profile (cache 5 phút)
- Health profile (cache 10 phút)
- Feed posts (cache 1 phút)

**Không nên cache**:
- Cart items (real-time)
- Order status (real-time)

---

### 9.3 Performance Tips

1. **Lazy loading images**: Load ảnh khi cần thiết
2. **Debounce search**: Đợi user ngừng gõ 300ms
3. **Background sync**: Sync cart/order khi mở app
4. **Compress images**: Resize ảnh trước khi upload

---

## 10. Future API Endpoints (TODO)

### Các API cần implement thêm

#### Social Features
- `POST /api/PostAPI/like` - Like bài viết
- `POST /api/PostAPI/comment` - Comment bài viết
- `POST /api/PostAPI/share` - Share bài viết
- `GET /api/PostAPI/user/{userId}` - Lấy posts của user
- `DELETE /api/PostAPI/{id}` - Xóa bài viết

#### Friend Features
- `GET /api/Friend/list` - Danh sách bạn bè
- `POST /api/Friend/request` - Gửi lời mời kết bạn
- `POST /api/Friend/accept` - Chấp nhận kết bạn
- `DELETE /api/Friend/{friendId}` - Hủy kết bạn

#### Health Features
- `POST /api/HealthProfile/allergies` - Cập nhật dị ứng
- `POST /api/HealthProfile/medications` - Quản lý thuốc đang dùng
- `GET /api/HealthProfile/history` - Lịch sử khám bệnh

#### Order Features
- `POST /api/OrderFoodAPI/AddToCart` - Thêm món vào giỏ
- `DELETE /api/OrderFoodAPI/RemoveFromCart` - Xóa món khỏi giỏ
- `POST /api/OrderFoodAPI/Checkout` - Thanh toán
- `GET /api/OrderFoodAPI/History` - Lịch sử đơn hàng

#### Food Features
- `GET /api/MonAn/list` - Danh sách món ăn
- `GET /api/MonAn/{id}` - Chi tiết món ăn
- `GET /api/MonAn/search` - Tìm kiếm món ăn
- `GET /api/MonAn/recommended` - Món ăn đề xuất

---

## 11. Database Schema Reference

### Key Tables

#### AspNetUsers
- Lưu thông tin user (Identity)
- Extend từ `ApplicationUser`
- Primary key: `Id` (string)

#### BaiDang
- Lưu posts social
- Foreign key: `NguoiDungId` -> AspNetUsers
- Foreign key: `Id_MonAn` -> MonAn (optional)

#### BaiThuoc
- Lưu bài thuốc
- Foreign key: `NguoiDungId` -> AspNetUsers

#### GioHang & GioHangChiTiet
- Lưu giỏ hàng
- 1 User -> 1 GioHang -> N GioHangChiTiet
- Foreign keys: 
  - GioHang.NguoiDungId -> AspNetUsers
  - GioHangChiTiet.GioHangId -> GioHang
  - GioHangChiTiet.MonAnId -> MonAn

#### HealthProfile
- Lưu hồ sơ sức khỏe
- Foreign key: `UserId` -> AspNetUsers
- 1-1 relationship

#### Friendships
- Lưu quan hệ bạn bè
- Columns: UserAId, UserBId, Status, CreatedAt

#### ArticleViews
- Track lượt xem bài thuốc
- Foreign keys: ArticleId -> BaiThuoc, UserId -> AspNetUsers

---

## 12. Configuration

### appsettings.json Structure

```json
{
  "ConnectionStrings": {
    "HotelWebConnection": "Server=...;Database=...;Trusted_Connection=True;"
  },
  "Authentication": {
    "Google": {
      "ClientId": "your-client-id",
      "ClientSecret": "your-secret"
    }
  },
  "Vnpay": {
    "TmnCode": "...",
    "HashSecret": "...",
    "Url": "https://sandbox.vnpayment.vn/..."
  }
}
```

---

## 13. Testing với Postman

### Collection Structure

```
Hotel_Web_API/
├── Authentication/
│   ├── Register
│   ├── Login Google
│   └── Logout
├── Social Feed/
│   ├── Get Feed
│   └── Get Post Detail
├── Order/
│   ├── Get Cart
│   └── Update Quantity
├── Health/
│   ├── Get Profile
│   ├── Update Personal Info
│   └── Update Chronic Conditions
└── Food Analysis/
    └── Analyze Image
```

### Environment Variables
```
base_url: https://localhost:7xxx
user_id: (sau khi login)
cookie: (sau khi login)
```

---

## 📞 Support & Contact

- **Backend Developer**: [Your Name]
- **Project**: Hotel Web - Health & Food Platform
- **Tech Stack**: ASP.NET Core 6/7, Entity Framework Core, SQL Server
- **Last Updated**: November 2025

---

## 📝 Change Log

### Version 1.0 (Current)
- ✅ Authentication với ASP.NET Identity
- ✅ Social Feed với mixing algorithm
- ✅ Food Order & Cart management
- ✅ Bài Thuốc CRUD
- ✅ Food Analysis với AI
- ✅ Health Profile management

### Version 1.1 (Planning)
- 🔲 Like/Comment/Share features
- 🔲 Friend management
- 🔲 Order checkout & payment
- 🔲 Real-time chat (SignalR)
- 🔲 Push notifications

---

**🎉 Chúc bạn code Flutter thành công!**
