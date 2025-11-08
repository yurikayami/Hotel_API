# 📝 API Quick Reference - Hotel Web

> Tài liệu tham khảo nhanh các endpoints, dành cho Flutter developers.

---

## 🔑 Authentication

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/Account/Register` | No | Đăng ký tài khoản |
| GET | `/Account/GoogleLogin` | No | Đăng nhập Google |
| POST | `/Account/Logout` | Yes | Đăng xuất |

---

## 📱 Social Feed (`/api/PostAPI`)

| Method | Endpoint | Auth | Description | Params |
|--------|----------|------|-------------|--------|
| GET | `/feed` | Optional | Lấy home feed | `page`, `pageSize` |
| GET | `/detail` | Optional | Chi tiết bài viết | `id`, `type` |

**Response Models**:
```typescript
interface FeedItem {
  id: string;
  type: "Post" | "BaiThuoc";
  content: string;
  imageUrl: string;
  ngayDang: Date;
  soBinhLuan: number;
  soChiaSe: number;
  luotThich: number;
  isLiked: boolean;
  authorId: string;
  authorName: string;
  avartar: string;
}
```

---

## 🛒 Order & Cart (`/api/OrderFoodAPI`)

| Method | Endpoint | Auth | Description | Body/Params |
|--------|----------|------|-------------|-------------|
| GET | `/GetCartItem` | Yes | Lấy giỏ hàng | - |
| POST | `/UpdateQuantities` | Yes | Cập nhật số lượng | `{chiTietId, soLuong}` |

**Cart Item Model**:
```typescript
interface CartItem {
  id: string;
  monAnId: string;
  tenMonAn: string;
  soLuong: number;
  donGia: number;
  thanhTien: number;
  imageUrl: string;
}
```

---

## 💊 Bài Thuốc (`/api/BaiThuocAPI`)

| Method | Endpoint | Auth | Content-Type | Description | Body |
|--------|----------|------|--------------|-------------|------|
| POST | `/create` | No | `multipart/form-data` | Tạo bài thuốc | `Ten`, `MoTa`, `Image` |

**Form Data**:
```typescript
{
  Ten: string;          // Required
  MoTa?: string;
  HuongDanSuDung?: string;
  Image?: File;
}
```

---

## 🍲 Food Analysis (`/api/FoodAnalysis`)

| Method | Endpoint | Auth | Content-Type | Description | Body |
|--------|----------|------|--------------|-------------|------|
| POST | `/analyze` | No | `multipart/form-data` | Phân tích ảnh món ăn | `image`, `userId`, `mealType` |

**Form Data**:
```typescript
{
  image: File;         // Required
  userId: string;      // Required
  mealType?: "breakfast" | "lunch" | "dinner" | "snack";
}
```

**Response**:
```typescript
interface AnalysisResult {
  success: boolean;
  imageUrl: string;
  prediction: {
    predicted_label: string;
    confidence: number;
    nutrition: {
      calories: number;
      protein: number;
      carbs: number;
      fat: number;
      fiber: number;
      mealType: string;
    };
  };
  planAdvice: {
    isWithinCalorieLimit: boolean;
    remainingCalories: number;
    message: string;
    recommendations: string[];
  };
}
```

---

## 🏥 Health Profile (`/api/HealthProfile`)

| Method | Endpoint | Auth | Description | Body |
|--------|----------|------|-------------|------|
| GET | `/` | Yes | Lấy hồ sơ sức khỏe | - |
| GET | `/completion` | Yes | Độ hoàn thiện hồ sơ | - |
| POST | `/personal-info` | Yes | Cập nhật thông tin cá nhân | PersonalInfoDto |
| POST | `/chronic-conditions` | Yes | Cập nhật bệnh mãn tính | ChronicConditionsDto |

**DTOs**:
```typescript
interface PersonalInfoDto {
  fullName?: string;
  age?: number;
  gender?: string;
  dateOfBirth?: Date;
  bloodType?: string;
  weight?: number;
  height?: number;
  activityLevel?: string;
}

interface ChronicConditionsDto {
  hasDiabetes?: boolean;
  hasHypertension?: boolean;
  hasAsthma?: boolean;
  hasHeartDisease?: boolean;
  otherDiseases?: string;
}
```

---

## 🍽️ Món Ăn (`/MonAn`)

| Method | Endpoint | Auth | Description | Params |
|--------|----------|------|-------------|--------|
| GET | `/Index` | No | Danh sách món ăn | - |
| GET | `/Detail/{id}` | No | Chi tiết món ăn | `id` |
| GET | `/GetPrice` | No | Lấy giá món ăn | `monAnId` |

---

## 📊 Response Status Codes

| Code | Meaning | Usage |
|------|---------|-------|
| 200 | OK | Success |
| 201 | Created | Resource created successfully |
| 204 | No Content | Deleted successfully |
| 400 | Bad Request | Invalid input |
| 401 | Unauthorized | Not logged in |
| 403 | Forbidden | No permission |
| 404 | Not Found | Resource not found |
| 500 | Server Error | Internal error |

---

## 🔧 Common Query Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `page` | int | 1 | Số trang |
| `pageSize` | int | 10 | Số items/trang |
| `search` | string | null | Từ khóa tìm kiếm |
| `sortBy` | string | "CreatedAt" | Trường để sort |
| `descending` | bool | true | Sort giảm dần |

---

## 🌐 Base URLs

| Environment | URL |
|-------------|-----|
| Development | `https://localhost:7xxx` |
| Staging | `https://staging.yourdomain.com` |
| Production | `https://api.yourdomain.com` |

---

## 🔐 Authentication Headers

### Cookie-based (Current)
```
Cookie: .AspNetCore.Identity.Application=xxx
```

### Bearer Token (If implemented)
```
Authorization: Bearer {access_token}
```

---

## 📦 Flutter HTTP Examples

### GET Request
```dart
final response = await http.get(
  Uri.parse('$baseUrl/api/PostAPI/feed?page=1&pageSize=10'),
  headers: {'Cookie': cookie},
);

if (response.statusCode == 200) {
  final data = jsonDecode(response.body);
  // Process data
}
```

### POST Request with JSON
```dart
final response = await http.post(
  Uri.parse('$baseUrl/api/OrderFoodAPI/UpdateQuantities'),
  headers: {
    'Content-Type': 'application/json',
    'Cookie': cookie,
  },
  body: jsonEncode({
    'chiTietId': 'guid',
    'soLuong': 2,
  }),
);
```

### POST Request with File
```dart
var request = http.MultipartRequest(
  'POST',
  Uri.parse('$baseUrl/api/BaiThuocAPI/create'),
);

request.fields['Ten'] = 'Bài thuốc ABC';
request.fields['MoTa'] = 'Mô tả...';
request.files.add(
  await http.MultipartFile.fromPath('Image', imagePath),
);

var response = await request.send();
```

---

## 🐛 Common Errors

### CORS Error
```json
{
  "error": "CORS policy: No 'Access-Control-Allow-Origin' header"
}
```
**Solution**: Đảm bảo CORS được config trong backend

### 401 Unauthorized
```json
{
  "error": "Bạn cần đăng nhập để thực hiện thao tác này"
}
```
**Solution**: Gọi login trước, lưu cookie/token

### 400 Bad Request
```json
{
  "errors": {
    "Ten": ["Tên không được để trống"]
  }
}
```
**Solution**: Kiểm tra validation rules

---

## 💡 Tips & Tricks

### Pagination Best Practice
```dart
// Load more khi scroll đến cuối
ScrollController _scrollController = ScrollController();

void initState() {
  _scrollController.addListener(() {
    if (_scrollController.position.pixels == 
        _scrollController.position.maxScrollExtent) {
      loadMorePosts();
    }
  });
}
```

### Image Caching
```dart
// Sử dụng cached_network_image package
CachedNetworkImage(
  imageUrl: '$baseUrl${item.imageUrl}',
  placeholder: (context, url) => CircularProgressIndicator(),
  errorWidget: (context, url, error) => Icon(Icons.error),
)
```

### Retry Logic
```dart
Future<T> retryRequest<T>(
  Future<T> Function() request, 
  {int maxAttempts = 3}
) async {
  for (int i = 0; i < maxAttempts; i++) {
    try {
      return await request();
    } catch (e) {
      if (i == maxAttempts - 1) rethrow;
      await Future.delayed(Duration(seconds: 2));
    }
  }
  throw Exception('Max retries exceeded');
}
```

---

## 📚 Related Documentation

- [Full API Documentation](./API_DOCUMENTATION.md)
- [Development Guide](./API_DEVELOPMENT_GUIDE.md)
- [Design Changes](./DESIGN_CHANGES.md)

---

**Last Updated**: November 2025
