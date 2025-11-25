# API Guide - Bài Thuốc (Medicine/Health Tips)

## 📌 Tổng Quan

Endpoint này cho phép lấy **danh sách bài thuốc (hướng dẫn sức khỏe)** của người dùng với support **infinite scroll**.

---

## 🔐 Authentication

**Riêng tư (My Medicine)**: ✅ Yêu cầu đăng nhập  
**Công khai (Public Medicine)**: ❌ Không yêu cầu

JWT Bearer Token:
```
Authorization: Bearer <YOUR_JWT_TOKEN>
```

---

## 📍 Endpoints

### 1. Lấy bài thuốc của chính mình (Authenticated)

```
GET /api/BaiThuocAPI/user/myMedicine
```

**Bắt buộc đăng nhập** ✅

---

### 2. Lấy bài thuốc của người dùng (Public)

```
GET /api/BaiThuocAPI/public/{userId}/medicine
```

**Công khai, không cần đăng nhập** ❌

---

## 📋 Parameters

| Tên | Kiểu | Mặc định | Max | Bắt buộc | Mô tả |
|-----|------|---------|-----|----------|-------|
| `offset` | `int` | `0` | - | ❌ | Số bài thuốc đã skip (phục vụ pagination) |
| `limit` | `int` | `10` | `50` | ❌ | Số bài thuốc trả về mỗi request |
| `userId` | `string` | - | - | ✅ | ID của người dùng (chỉ dùng cho public endpoint) |

### Validation Rules:
- `offset` < 0 → tự động set về 0
- `limit` < 1 → tự động set về 10
- `limit` > 50 → tự động clamp về 50
- `userId` không được để trống (public endpoint)

---

## 📤 Request Examples

### 1. Lấy Bài Thuốc Của Mình

#### cURL:
```bash
curl -X GET \
  "https://localhost:7135/api/BaiThuocAPI/user/myMedicine?offset=0&limit=10" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json"
```

#### JavaScript/Fetch:
```javascript
const token = 'YOUR_JWT_TOKEN';
const offset = 0;
const limit = 10;

fetch(`https://localhost:7135/api/BaiThuocAPI/user/myMedicine?offset=${offset}&limit=${limit}`, {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
})
.then(res => res.json())
.then(data => console.log(data))
.catch(err => console.error(err));
```

#### Python/Requests:
```python
import requests

token = "YOUR_JWT_TOKEN"
headers = {
    "Authorization": f"Bearer {token}",
    "Content-Type": "application/json"
}

url = "https://localhost:7135/api/BaiThuocAPI/user/myMedicine"
params = {
    "offset": 0,
    "limit": 10
}

response = requests.get(url, headers=headers, params=params)
data = response.json()
print(data)
```

#### Dart/Flutter:
```dart
import 'package:http/http.dart' as http;
import 'dart:convert';

Future<Map<String, dynamic>> getMyMedicines({
  required String token,
  int offset = 0,
  int limit = 10,
}) async {
  final url = Uri.parse(
    'https://localhost:7135/api/BaiThuocAPI/user/myMedicine?offset=$offset&limit=$limit'
  );

  final response = await http.get(
    url,
    headers: {
      'Authorization': 'Bearer $token',
      'Content-Type': 'application/json',
    },
  );

  if (response.statusCode == 200) {
    return jsonDecode(response.body);
  } else if (response.statusCode == 401) {
    throw Exception('Unauthorized - Token không hợp lệ hoặc hết hạn');
  } else {
    throw Exception('Lỗi: ${response.statusCode}');
  }
}
```

### 2. Lấy Bài Thuốc Công Khai Của Người Dùng

#### cURL:
```bash
curl -X GET \
  "https://localhost:7135/api/BaiThuocAPI/public/user-123/medicine?offset=0&limit=10" \
  -H "Content-Type: application/json"
```

#### JavaScript/Fetch:
```javascript
const userId = 'user-123';
const offset = 0;
const limit = 10;

fetch(`https://localhost:7135/api/BaiThuocAPI/public/${userId}/medicine?offset=${offset}&limit=${limit}`, {
  method: 'GET',
  headers: {
    'Content-Type': 'application/json'
  }
})
.then(res => res.json())
.then(data => console.log(data))
.catch(err => console.error(err));
```

#### Python/Requests:
```python
import requests

userId = "user-123"
url = f"https://localhost:7135/api/BaiThuocAPI/public/{userId}/medicine"
params = {
    "offset": 0,
    "limit": 10
}

response = requests.get(url, params=params)
data = response.json()
print(data)
```

#### Dart/Flutter:
```dart
Future<Map<String, dynamic>> getPublicMedicines({
  required String userId,
  int offset = 0,
  int limit = 10,
}) async {
  final url = Uri.parse(
    'https://localhost:7135/api/BaiThuocAPI/public/$userId/medicine?offset=$offset&limit=$limit'
  );

  final response = await http.get(url);

  if (response.statusCode == 200) {
    return jsonDecode(response.body);
  } else {
    throw Exception('Lỗi: ${response.statusCode}');
  }
}
```

---

## 📥 Response Format

### Success Response (Status 200):
```json
{
  "success": true,
  "message": "Danh sách bài thuốc của bạn",
  "data": {
    "medicines": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "ten": "Hướng dẫn điều trị cảm cúm",
        "moTa": "Cảm cúm là bệnh do virus gây ra...",
        "huongDanSuDung": "Uống thuốc 3 lần/ngày sau ăn cơm",
        "ngayTao": "2025-11-18T10:30:00",
        "image": "https://example.com/uploads/baithuoc/image.jpg",
        "soLuotThich": 25,
        "soLuotXem": 150,
        "authorId": "user-123",
        "authorName": "Dr. Nguyễn Văn A",
        "authorAvatar": "https://example.com/avatars/doctor.jpg"
      },
      {
        "id": "660e8400-e29b-41d4-a716-446655440111",
        "ten": "Chế độ ăn uống lành mạnh",
        "moTa": "Một chế độ ăn uống cân bằng...",
        "huongDanSuDung": "Ăn đầy đủ các chất dinh dưỡng",
        "ngayTao": "2025-11-17T15:45:00",
        "image": null,
        "soLuotThich": 45,
        "soLuotXem": 320,
        "authorId": "user-456",
        "authorName": "Nutritionist B",
        "authorAvatar": null
      }
    ],
    "hasMore": true
  },
  "errors": []
}
```

### Error Response (Status 401):
```json
{
  "success": false,
  "message": "Bạn cần đăng nhập để xem bài thuốc của mình",
  "data": null,
  "errors": []
}
```

### Error Response (Status 400):
```json
{
  "success": false,
  "message": "ID người dùng không được để trống",
  "data": null,
  "errors": []
}
```

### Error Response (Status 500):
```json
{
  "success": false,
  "message": "Có lỗi xảy ra khi lấy danh sách bài thuốc",
  "data": null,
  "errors": [
    "Exception message..."
  ]
}
```

---

## 📊 Response Fields

### Main Response Object:
| Field | Kiểu | Mô tả |
|-------|------|-------|
| `success` | `boolean` | Trạng thái request (true = thành công) |
| `message` | `string` | Mô tả kết quả |
| `data` | `object` | Dữ liệu bài thuốc (xem chi tiết dưới) |
| `errors` | `array` | Danh sách lỗi (nếu có) |

### Data Object:
| Field | Kiểu | Mô tả |
|-------|------|-------|
| `medicines` | `array<Medicine>` | Danh sách bài thuốc |
| `hasMore` | `boolean` | `true` nếu còn bài thuốc, `false` nếu là trang cuối |

### Medicine Object:
| Field | Kiểu | Mô tả |
|-------|------|-------|
| `id` | `string` (UUID) | ID bài thuốc |
| `ten` | `string` | Tiêu đề bài thuốc |
| `moTa` | `string` | Mô tả chi tiết |
| `huongDanSuDung` | `string` | Hướng dẫn sử dụng |
| `ngayTao` | `datetime` | Ngày tạo bài thuốc |
| `image` | `string` \| `null` | URL ảnh bài thuốc |
| `soLuotThich` | `integer` | Số lượt like |
| `soLuotXem` | `integer` | Số lượt xem |
| `authorId` | `string` | ID tác giả |
| `authorName` | `string` | Tên tác giả |
| `authorAvatar` | `string` \| `null` | URL avatar tác giả |

---

## 🔄 Infinite Scroll Implementation

### Logic:
1. Gọi API lần đầu với `offset=0, limit=10`
2. Nếu `hasMore=true`, khi user scroll tới cuối, increment `offset += limit`
3. Gọi API lại với `offset=10, limit=10` (và tiếp tục...)
4. Dừng khi `hasMore=false`

### Dart/Flutter Example:
```dart
import 'package:http/http.dart' as http;
import 'dart:convert';

class MedicineService {
  static const String baseUrl = 'https://localhost:7135';
  static String? _token;

  static void setToken(String token) {
    _token = token;
  }

  // Lấy bài thuốc của mình
  static Future<Map<String, dynamic>> getMyMedicines({
    required int offset,
    int limit = 10,
  }) async {
    final url = Uri.parse(
      '$baseUrl/api/BaiThuocAPI/user/myMedicine?offset=$offset&limit=$limit'
    );
    
    final response = await http.get(
      url,
      headers: {
        'Authorization': 'Bearer $_token',
        'Content-Type': 'application/json',
      },
    );

    if (response.statusCode == 200) {
      return jsonDecode(response.body);
    }
    throw Exception('Lỗi tải bài thuốc');
  }

  // Lấy bài thuốc công khai
  static Future<Map<String, dynamic>> getPublicMedicines({
    required String userId,
    required int offset,
    int limit = 10,
  }) async {
    final url = Uri.parse(
      '$baseUrl/api/BaiThuocAPI/public/$userId/medicine?offset=$offset&limit=$limit'
    );
    
    final response = await http.get(url);

    if (response.statusCode == 200) {
      return jsonDecode(response.body);
    }
    throw Exception('Lỗi tải bài thuốc');
  }
}

class MedicineScreen extends StatefulWidget {
  final bool isPublic;
  final String? userId;

  const MedicineScreen({
    this.isPublic = false,
    this.userId,
  });

  @override
  State<MedicineScreen> createState() => _MedicineScreenState();
}

class _MedicineScreenState extends State<MedicineScreen> {
  List<Map<String, dynamic>> medicines = [];
  int offset = 0;
  const int limit = 10;
  bool isLoading = false;
  bool hasMore = true;
  final ScrollController _scrollController = ScrollController();

  @override
  void initState() {
    super.initState();
    _loadMedicines();
    _scrollController.addListener(_onScroll);
  }

  void _onScroll() {
    if (_scrollController.position.pixels == 
        _scrollController.position.maxScrollExtent) {
      if (hasMore && !isLoading) {
        _loadMedicines();
      }
    }
  }

  Future<void> _loadMedicines() async {
    if (isLoading || !hasMore) return;

    setState(() => isLoading = true);

    try {
      late Map<String, dynamic> data;

      if (widget.isPublic) {
        data = await MedicineService.getPublicMedicines(
          userId: widget.userId!,
          offset: offset,
          limit: limit,
        );
      } else {
        data = await MedicineService.getMyMedicines(
          offset: offset,
          limit: limit,
        );
      }

      if (data['success']) {
        setState(() {
          medicines.addAll(
            List<Map<String, dynamic>>.from(data['data']['medicines'])
          );
          offset += limit;
          hasMore = data['data']['hasMore'] ?? false;
          isLoading = false;
        });
      } else {
        throw Exception(data['message']);
      }
    } catch (e) {
      setState(() => isLoading = false);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Lỗi: ${e.toString()}')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(
          widget.isPublic ? 'Bài thuốc công khai' : 'Bài thuốc của tôi'
        ),
      ),
      body: medicines.isEmpty && !isLoading
          ? Center(child: Text('Chưa có bài thuốc nào'))
          : ListView.builder(
              controller: _scrollController,
              itemCount: medicines.length + (isLoading ? 1 : 0),
              itemBuilder: (context, index) {
                if (index == medicines.length) {
                  return Padding(
                    padding: EdgeInsets.all(16),
                    child: CircularProgressIndicator(),
                  );
                }

                final medicine = medicines[index];
                return MedicineCard(medicine: medicine);
              },
            ),
    );
  }

  @override
  void dispose() {
    _scrollController.dispose();
    super.dispose();
  }
}

class MedicineCard extends StatelessWidget {
  final Map<String, dynamic> medicine;

  const MedicineCard({required this.medicine});

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: EdgeInsets.all(8),
      child: Padding(
        padding: EdgeInsets.all(12),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Header
            Row(
              children: [
                CircleAvatar(
                  backgroundImage: medicine['authorAvatar'] != null
                      ? NetworkImage(medicine['authorAvatar'])
                      : null,
                  child: medicine['authorAvatar'] == null
                      ? Icon(Icons.person)
                      : null,
                ),
                SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        medicine['authorName'] ?? 'Unknown',
                        style: TextStyle(fontWeight: FontWeight.bold),
                      ),
                      Text(
                        medicine['ngayTao'] ?? '',
                        style: TextStyle(fontSize: 12, color: Colors.grey),
                      ),
                    ],
                  ),
                ),
              ],
            ),
            SizedBox(height: 12),
            // Title
            Text(
              medicine['ten'] ?? '',
              style: TextStyle(
                fontSize: 16,
                fontWeight: FontWeight.bold,
              ),
            ),
            SizedBox(height: 8),
            // Description
            if (medicine['moTa'] != null)
              Text(
                medicine['moTa'],
                maxLines: 3,
                overflow: TextOverflow.ellipsis,
              ),
            SizedBox(height: 8),
            // Usage
            if (medicine['huongDanSuDung'] != null)
              Container(
                padding: EdgeInsets.all(8),
                decoration: BoxDecoration(
                  color: Colors.blue.withOpacity(0.1),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Hướng dẫn:',
                      style: TextStyle(fontWeight: FontWeight.bold),
                    ),
                    Text(medicine['huongDanSuDung']),
                  ],
                ),
              ),
            if (medicine['image'] != null)
              Padding(
                padding: EdgeInsets.only(top: 12),
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(8),
                  child: Image.network(
                    medicine['image'],
                    fit: BoxFit.cover,
                    errorBuilder: (context, error, stackTrace) {
                      return Container(
                        height: 200,
                        color: Colors.grey[300],
                        child: Icon(Icons.broken_image),
                      );
                    },
                  ),
                ),
              ),
            SizedBox(height: 12),
            // Stats
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceAround,
              children: [
                Row(
                  children: [
                    Icon(Icons.favorite_border),
                    SizedBox(width: 4),
                    Text('${medicine['soLuotThich'] ?? 0}'),
                  ],
                ),
                Row(
                  children: [
                    Icon(Icons.visibility),
                    SizedBox(width: 4),
                    Text('${medicine['soLuotXem'] ?? 0}'),
                  ],
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
```

---

## ⚠️ HTTP Status Codes

| Code | Ý nghĩa | Hành động |
|------|---------|----------|
| `200` | ✅ Thành công | Xử lý dữ liệu bình thường |
| `400` | ❌ Request sai | Kiểm tra userId hoặc parameters |
| `401` | ❌ Chưa đăng nhập | Redirect user tới login, lấy token mới |
| `500` | ❌ Lỗi server | Báo lỗi, retry sau vài giây |

---

## 💾 Database Schema

```sql
CREATE TABLE [dbo].[BaiThuoc](
	[Id] [uniqueidentifier] NOT NULL,
	[Ten] [nvarchar](max) NOT NULL,
	[MoTa] [nvarchar](max) NULL,
	[HuongDanSuDung] [nvarchar](max) NULL,
	[NguoiDungId] [nvarchar](450) NULL,
	[NgayTao] [datetime2](7) NOT NULL,
	[Image] [nvarchar](max) NULL,
	[SoLuotThich] [int] NULL,
	[SoLuotXem] [int] NULL,
	[TrangThai] [int] NULL,
	PRIMARY KEY ([Id])
)
```

### Các trường:
- **Id**: UUID của bài thuốc
- **Ten**: Tiêu đề bài thuốc (bắt buộc)
- **MoTa**: Mô tả chi tiết
- **HuongDanSuDung**: Hướng dẫn sử dụng
- **NguoiDungId**: FK tới AspNetUsers
- **NgayTao**: Timestamp tạo bài
- **Image**: Đường dẫn ảnh
- **SoLuotThich**: Số lượt like
- **SoLuotXem**: Số lượt xem
- **TrangThai**: Trạng thái (1=active, 0=inactive)

---

## 🚨 Error Handling

### Token hết hạn:
```dart
if (response.statusCode == 401) {
  await storage.delete(key: 'jwt_token');
  Navigator.pushReplacementNamed(context, '/login');
}
```

### Network timeout:
```dart
try {
  await getMedicines().timeout(Duration(seconds: 10));
} on TimeoutException {
  print('Request timeout - kiểm tra kết nối internet');
}
```

### Retry logic:
```dart
Future<Map> getWithRetry({int maxRetries = 3}) async {
  for (int i = 0; i < maxRetries; i++) {
    try {
      return await MedicineService.getMyMedicines(offset: offset, limit: limit);
    } catch (e) {
      if (i == maxRetries - 1) rethrow;
      await Future.delayed(Duration(seconds: 2 * (i + 1)));
    }
  }
}
```

---

## 💡 Best Practices

1. **Cache locally** - Lưu medicines vào SQLite/Hive để offline access
2. **Pagination params** - Khởi tạo `limit=20` để balance tốc độ
3. **Error UI** - Hiển thị spinner khi loading, toast khi error
4. **Image optimization** - Dùng `cached_network_image` package
5. **State management** - Dùng Provider/Riverpod/Bloc
6. **Lazy loading** - Không render tất cả items một lúc
7. **Input validation** - Validate userId format trước gọi API

---

## 🔗 Related Endpoints

- `GET /api/BaiThuocAPI` - Danh sách bài thuốc (pagination)
- `GET /api/BaiThuocAPI/{id}` - Chi tiết bài thuốc
- `POST /api/BaiThuocAPI/create` - Tạo bài thuốc mới

---

## 📞 Support

**Issues?** Liên hệ: backend@example.com  
**Last Updated**: 18/11/2025  
**API Version**: 1.0
