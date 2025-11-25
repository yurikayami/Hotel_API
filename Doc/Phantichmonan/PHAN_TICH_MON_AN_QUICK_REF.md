# 📖 Phân Tích Món Ăn - Quick Reference

> Tài liệu tham khảo nhanh cho Food Analysis API

---

## 🎯 Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/FoodAnalysis/analyze` | No | Phân tích ảnh |
| GET | `/api/FoodAnalysis/history/{userId}` | No | Lấy lịch sử |
| DELETE | `/api/FoodAnalysis/history/{id}` | No | Xóa bản ghi |

---

## 📤 POST - Phân Tích Ảnh

### Request

```bash
POST /api/FoodAnalysis/analyze HTTP/1.1
Content-Type: multipart/form-data

image=[FILE]
userId=user-123
mealType=lunch
```

### Response (200)

```json
{
  "id": 1,
  "userId": "user-123",
  "imagePath": "https://localhost:7xxx/uploads/abc123.jpg",
  "foodName": "Phở Bò",
  "confidence": 0.95,
  "calories": 450,
  "protein": 25,
  "fat": 12,
  "carbs": 60,
  "mealType": "lunch",
  "advice": "Bữa ăn này phù hợp với phác đồ...",
  "createdAt": "2025-11-09T10:30:00Z",
  "details": [
    {
      "label": "Phở",
      "weight": 300,
      "confidence": 0.92,
      "calories": 280,
      "protein": 12,
      "fat": 5,
      "carbs": 48
    }
  ]
}
```

### Errors

| Code | Message | Solution |
|------|---------|----------|
| 400 | "User ID is required" | Gửi `userId` |
| 400 | "Invalid image file" | Kiểm tra file type |
| 400 | "Không tìm thấy phác đồ" | Tạo health plan trước |
| 500 | Python API error | Kiểm tra Flask server |

---

## 📥 GET - Lịch Sử

### Request

```bash
GET /api/FoodAnalysis/history/user-123 HTTP/1.1
```

### Response (200)

```json
[
  {
    "id": 2,
    "image": "https://localhost:7xxx/uploads/xyz.jpg",
    "comfident": 0.94,
    "foodName": "Cơm Gà",
    "calories": 520,
    "createdAt": "2025-11-09T12:15:00Z",
    "mealType": "lunch",
    "protein": 28,
    "fat": 18,
    "carbs": 65,
    "details": [...]
  }
]
```

---

## 🗑️ DELETE - Xóa

### Request

```bash
DELETE /api/FoodAnalysis/history/1 HTTP/1.1
```

### Response (204 No Content)

```
(Empty)
```

---

## 🎯 Models

### PredictionHistory

```dart
class PredictionHistory {
  int id;
  String userId;
  String imagePath;
  String foodName;
  double confidence;      // 0.0 - 1.0
  double calories;        // kcal
  double protein;         // g
  double fat;             // g
  double carbs;           // g
  String? mealType;       // breakfast/lunch/dinner/snack
  String? advice;         // AI recommendation
  DateTime createdAt;
  List<PredictionDetail> details;
}
```

### PredictionDetail

```dart
class PredictionDetail {
  String label;           // "Phở", "Thịt", "Rau"
  double weight;          // g
  double confidence;      // detection accuracy
  double calories;
  double protein;
  double fat;
  double carbs;
}
```

---

## 🔄 Flow

```
Upload Image
    ↓
Validate & Save File
    ↓
Call Python API (detect food)
    ↓
Get User Health Plan
    ↓
Call Gemini (get advice)
    ↓
Save to Database
    ↓
Return Result + Details
```

---

## 💻 Flutter Implementation

### 1. Select & Analyze

```dart
import 'package:image_picker/image_picker.dart';
import 'package:http/http.dart' as http;

final picker = ImagePicker();
final image = await picker.pickImage(source: ImageSource.gallery);

if (image != null) {
  var request = http.MultipartRequest(
    'POST',
    Uri.parse('https://domain.com/api/FoodAnalysis/analyze'),
  );
  
  request.fields['userId'] = userId;
  request.fields['mealType'] = 'lunch';
  request.files.add(
    await http.MultipartFile.fromPath('image', image.path),
  );
  
  var response = await request.send();
  var responseData = await response.stream.bytesToString();
  
  if (response.statusCode == 200) {
    final result = jsonDecode(responseData);
    print('Food: ${result['foodName']}');
    print('Calories: ${result['calories']}');
    print('Advice: ${result['advice']}');
  }
}
```

### 2. Show History

```dart
final response = await http.get(
  Uri.parse('https://domain.com/api/FoodAnalysis/history/$userId'),
);

if (response.statusCode == 200) {
  final List<dynamic> history = jsonDecode(response.body);
  
  ListView.builder(
    itemCount: history.length,
    itemBuilder: (context, index) {
      final item = history[index];
      return ListTile(
        leading: Image.network(item['image'], width: 50, height: 50),
        title: Text(item['foodName']),
        subtitle: Text('${item['calories']} kcal'),
        trailing: Text(item['createdAt']),
      );
    },
  );
}
```

### 3. Delete History

```dart
await http.delete(
  Uri.parse('https://domain.com/api/FoodAnalysis/history/$id'),
);
```

---

## ⚙️ Configuration

### appsettings.json

```json
{
  "GoogleGemini": {
    "ApiKey": "AIzaSy..."
  }
}
```

### Python Server

```bash
# Run Flask API (port 5000)
python app.py
```

---

## 🐛 Common Issues

| Issue | Solution |
|-------|----------|
| Python connection error | Kiểm tra Flask server chạy? (port 5000) |
| Gemini error | Kiểm tra API key trong appsettings |
| File not saved | Kiểm tra wwwroot/uploads folder tồn tại |
| No health plan | Tạo health plan trước analyze |
| Image too large | Compress image trước upload |

---

## 🔐 Security

- ✅ File validation (check content-type)
- ✅ Unique filename (GUID-based)
- ✅ File stored outside web root
- ✅ Old files cleanup
- ⚠️ No auth required (consider adding for production)

---

## 📊 Database

**Tables**:
- `PredictionHistory` - Bản ghi phân tích
- `PredictionDetail` - Chi tiết từng thành phần

**Key Queries**:
```sql
-- Lấy hôm nay
SELECT * FROM PredictionHistory 
WHERE UserId = @userId 
  AND CAST(CreatedAt AS DATE) = CAST(GETDATE() AS DATE)
  
-- Tổng calories
SELECT SUM(Calories) FROM PredictionHistory 
WHERE UserId = @userId 
  AND CAST(CreatedAt AS DATE) = CAST(GETDATE() AS DATE)
```

---

## 📱 Response Times

| Operation | Expected Time |
|-----------|----------------|
| Image Upload | 1-2s |
| Python Detection | 2-3s |
| Gemini Response | 2-5s |
| **Total** | **5-10s** |

---

## 🎓 Learning Resources

- [Gemini API](https://ai.google.dev/docs)
- [Flask Documentation](https://flask.palletsprojects.com)
- [Image Upload ASP.NET](https://docs.microsoft.com/aspnet/core/mvc/models/file-uploads)
- [Flutter HTTP](https://pub.dev/packages/http)

---

**Last Updated**: November 9, 2025
