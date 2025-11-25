# 🍲 Phân Tích Món Ăn (Food Analysis) API

> Tài liệu chi tiết về hệ thống phân tích dinh dưỡng từ ảnh món ăn bằng AI

---

## 📋 Mục Lục

1. [Giới Thiệu](#1-giới-thiệu)
2. [Kiến Trúc Hệ Thống](#2-kiến-trúc-hệ-thống)
3. [API Endpoints](#3-api-endpoints)
4. [Flow Chi Tiết](#4-flow-chi-tiết)
5. [Models & DTOs](#5-models--dtos)
6. [Services](#6-services)
7. [Database Schema](#7-database-schema)
8. [Integration cho Flutter](#8-integration-cho-flutter)
9. [Troubleshooting](#9-troubleshooting)

---

## 1. Giới Thiệu

### Tính Năng Chính

Hệ thống **Phân Tích Món Ăn** cung cấp:

✅ **Nhận Diện Món Ăn từ Ảnh**: Sử dụng Python AI model để detect món ăn  
✅ **Phân Tích Dinh Dưỡng**: Trích xuất calories, protein, fat, carbs  
✅ **So Sánh với Phác Đồ**: Kiểm tra phù hợp với kế hoạch dinh dưỡng của user  
✅ **Lời Khuyên AI**: Gemini AI đưa ra gợi ý cá nhân hóa  
✅ **Lịch Sử Phân Tích**: Lưu và xem lại các lần phân tích trước  

### Use Cases

```
User Flow:
1. User chụp ảnh bát phở
   ↓
2. Upload lên backend
   ↓
3. Python API detect: "Phở Bò"
   ↓
4. Extract dinh dưỡng: 450kcal, 25g protein...
   ↓
5. Lấy health plan của user
   ↓
6. Gọi Gemini để đưa ra lời khuyên
   ↓
7. Trả về kết quả + advice cho user
   ↓
8. Lưu vào PredictionHistory
```

---

## 2. Kiến Trúc Hệ Thống

### Thành Phần Chính

```
┌─────────────────────────────────────────────────────────┐
│                    Flutter App                          │
│            (Select Photo → Upload Image)                │
└────────────────────┬────────────────────────────────────┘
                     │ HTTP POST /api/FoodAnalysis/analyze
                     ↓
┌─────────────────────────────────────────────────────────┐
│               ASP.NET Core Backend                       │
│                                                          │
│  ┌─────────────────────────────────────────────────┐   │
│  │  FoodAnalysisController                          │   │
│  │  - Validate image                               │   │
│  │  - Save file                                    │   │
│  │  - Call Python API                             │   │
│  │  - Get health plan                             │   │
│  │  - Call NutritionService                       │   │
│  │  - Save to database                            │   │
│  └─────────────────────────────────────────────────┘   │
│                     │                                   │
│         ┌───────────┼───────────┐                       │
│         ↓           ↓           ↓                       │
│    ┌────────┐  ┌────────┐  ┌──────────┐               │
│    │ Python │  │Gemini  │  │ Database │               │
│    │  API   │  │  API   │  │ (SQL)    │               │
│    │(port   │  │(Google │  │          │               │
│    │ 5000)  │  │ Gen AI)│  │          │               │
│    └────────┘  └────────┘  └──────────┘               │
│                                                        │
│  ┌─────────────────────────────────────────────────┐  │
│  │  NutritionService                               │  │
│  │  - Analyze nutrition                            │  │
│  │  - Get Gemini advice                            │  │
│  │  - Parse health plan                            │  │
│  └─────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

### Technology Stack

- **AI Detection**: Python Flask API (custom trained model)
- **Nutrition Advice**: Google Gemini 2.0 Flash
- **Storage**: SQL Server + File System (wwwroot/uploads)
- **Backend Framework**: ASP.NET Core 6/7
- **ORM**: Entity Framework Core

---

## 3. API Endpoints

### 3.1 Phân Tích Ảnh Món Ăn

**Endpoint**: `POST /api/FoodAnalysis/analyze`

**Description**: Upload ảnh, phân tích dinh dưỡng và nhận lời khuyên

**Authentication**: Not required (nhưng `userId` là bắt buộc)

**Content-Type**: `multipart/form-data`

**Request Parameters (Form Data)**:

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `image` | File | ✅ | Ảnh món ăn (jpg, png, etc.) |
| `userId` | string | ✅ | ID của user |
| `mealType` | string | ❌ | "breakfast", "lunch", "dinner", "snack" |

**Request Example (cURL)**:

```bash
curl -X POST http://localhost:7xxx/api/FoodAnalysis/analyze \
  -F "image=@/path/to/dish.jpg" \
  -F "userId=user-123" \
  -F "mealType=lunch"
```

**Response Success** (200 OK):

```json
{
  "id": 1,
  "userId": "user-123",
  "imagePath": "https://localhost:7xxx/uploads/abc123def456.jpg",
  "foodName": "Phở Bò",
  "confidence": 0.95,
  "calories": 450,
  "protein": 25,
  "fat": 12,
  "carbs": 60,
  "mealType": "lunch",
  "advice": "Bữa ăn này phù hợp với phác đồ dinh dưỡng của bạn. Phở cung cấp protein tốt từ thịt bò và có độ cân bằng dinh dưỡng tốt. Hãy thêm rau xanh để tăng vitamin.",
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
    },
    {
      "label": "Thịt Bò",
      "weight": 100,
      "confidence": 0.89,
      "calories": 140,
      "protein": 22,
      "fat": 5,
      "carbs": 0
    },
    {
      "label": "Rau Thơm",
      "weight": 20,
      "confidence": 0.85,
      "calories": 30,
      "protein": 1,
      "fat": 0.5,
      "carbs": 6
    }
  ]
}
```

**Response Error Cases**:

**400 Bad Request** - Missing userId:
```json
"User ID is required"
```

**400 Bad Request** - Invalid image:
```json
"Invalid image file."
```

**400 Bad Request** - No health plan:
```json
"Không tìm thấy phác đồ của người dùng."
```

**500 Internal Server Error** - Python API error:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500,
  "detail": "Python API error: 500 - {error details}"
}
```

---

### 3.2 Lấy Lịch Sử Phân Tích

**Endpoint**: `GET /api/FoodAnalysis/history/{userId}`

**Description**: Lấy danh sách tất cả các lần phân tích của user

**Authentication**: Not required

**Path Parameters**:

| Parameter | Type | Description |
|-----------|------|-------------|
| `userId` | string | ID của user |

**Query Parameters**: Không có

**Response Success** (200 OK):

```json
[
  {
    "id": 2,
    "image": "https://localhost:7xxx/uploads/xyz789.jpg",
    "comfident": 0.94,
    "foodName": "Cơm Gà",
    "calories": 520,
    "createdAt": "2025-11-09T12:15:00Z",
    "mealType": "lunch",
    "protein": 28,
    "fat": 18,
    "carbs": 65,
    "details": [
      {
        "label": "Cơm",
        "weight": 200,
        "confidence": 0.93,
        "calories": 260,
        "protein": 6,
        "fat": 2,
        "carbs": 58
      },
      {
        "label": "Gà Luộc",
        "weight": 150,
        "confidence": 0.91,
        "calories": 230,
        "protein": 35,
        "fat": 8,
        "carbs": 0
      }
    ]
  },
  {
    "id": 1,
    "image": "https://localhost:7xxx/uploads/abc123def456.jpg",
    "comfident": 0.95,
    "foodName": "Phở Bò",
    "calories": 450,
    "createdAt": "2025-11-09T10:30:00Z",
    "mealType": "lunch",
    "protein": 25,
    "fat": 12,
    "carbs": 60,
    "details": [...]
  }
]
```

**Response Error** (200 OK - empty array):
```json
[]
```

---

### 3.3 Xóa Lịch Sử Phân Tích

**Endpoint**: `DELETE /api/FoodAnalysis/history/{id}`

**Description**: Xóa một bản ghi phân tích (bao gồm ảnh và dữ liệu DB)

**Authentication**: Not required

**Path Parameters**:

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | int | ID của PredictionHistory |

**Response Success** (204 No Content):
```
(Empty body)
```

**Response Error** (404 Not Found):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "..."
}
```

---

## 4. Flow Chi Tiết

### 4.1 Flow Phân Tích Ảnh

```
POST /api/FoodAnalysis/analyze
    ↓
[Step 1] Validate Input
  - Check userId not null
  - Check image file is valid (content-type: image/*)
    ↓
[Step 2] Save Physical File
  - Create folder: wwwroot/uploads/ nếu chưa tồn tại
  - Generate filename: Guid.NewGuid() + extension
  - Save file to disk
    ↓
[Step 3] Build Public URL
  - imageUrl = "https://domain.com/uploads/{filename}"
    ↓
[Step 4] Call Python API
  - POST to http://127.0.0.1:5000/predict
  - Send image file as multipart
  - Return: PredictionResult
    {
      predicted_label: "Phở Bò",
      confidence: 0.95,
      nutrition: { calories, protein, fat, carbs, mealType },
      details: [ { label, weight, cal, ... } ]
    }
    ↓
[Step 5] Get User Health Plan
  - Query: HealthPlans
    .Where(p => p.UserId == userId)
    .OrderByDescending(p => p.NgayTao)
    .FirstOrDefault()
  - If null → return error "Không tìm thấy phác đồ"
    ↓
[Step 6] Call NutritionService
  - Input: userId, mealType, mealCalories, foodName
  - Process:
    * Query latest HealthPlan
    * Call Gemini API with prompt
    * Return AI advice string
    ↓
[Step 7] Save to Database
  - Create PredictionHistory record:
    {
      UserId, ImagePath, FoodName, Confidence,
      Calories, Protein, Fat, Carbs, MealType,
      Advice, CreatedAt
    }
  - For each detail item, create PredictionDetail
    ↓
[Step 8] Return Response
  - Return complete PredictionHistory object (200 OK)
    ↓
[Step 9] Cleanup (Finally block)
  - If file still exists at URL, delete it
  - (Note: File already saved, so this usually does nothing)
```

### 4.2 Gemini Advice Generation

```
Input: foodName, condition, nutritionInfo, mealType, mealCalories
    ↓
Build Prompt:
  - System: "Bạn là chuyên gia dinh dưỡng..."
  - User question: Chi tiết món ăn + tình trạng sức khỏe
  - Yêu cầu: 
    1. Món này có phù hợp không?
    2. Gợi ý 1 món thay thế
    3. Không nêu số liệu
    ↓
POST to Gemini API:
  - URL: https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={API_KEY}
  - Body: { contents, generationConfig }
  - generationConfig: { temperature: 0.7, topP: 0.9, maxOutputTokens: 150 }
    ↓
Parse Response:
  - Extract: candidates[0].content.parts[0].text
  - Return as string
```

---

## 5. Models & DTOs

### 5.1 PredictionResult (Python Response)

```csharp
public class PredictionResult
{
    public JsonElement predicted_label { get; set; }  // Tên món ăn
    public double confidence { get; set; }            // 0.0 - 1.0
    public NutritionInfo nutrition { get; set; }     // Tổng dinh dưỡng
    public List<FoodDetail> details { get; set; }    // Chi tiết từng thành phần
}

public class NutritionInfo
{
    public double total_weight { get; set; }         // Tổng khối lượng (g)
    public double calories { get; set; }             // kcal
    public double fat { get; set; }                  // g
    public double carbs { get; set; }                // g
    public double protein { get; set; }              // g
    public string? mealType { get; set; }            // breakfast/lunch/dinner/snack
}

public class FoodDetail
{
    public string label { get; set; }                // Thành phần (e.g., "Phở", "Thịt")
    public double confidence { get; set; }           // Độ tự tin detect
    public double weight { get; set; }               // Khối lượng (g)
    public double cal { get; set; }                  // kcal
    public double fat { get; set; }                  // g
    public double carbs { get; set; }                // g
    public double protein { get; set; }              // g
}
```

### 5.2 PredictionHistory (Database Model)

```csharp
[Table("PredictionHistory")]
public class PredictionHistory
{
    [Key]
    public int Id { get; set; }                      // Primary key
    public string UserId { get; set; }               // Foreign key
    public string ImagePath { get; set; }            // Full URL
    public string FoodName { get; set; }             // "Phở Bò"
    public double Confidence { get; set; }           // 0.95
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Fat { get; set; }
    public double Carbs { get; set; }
    public string? MealType { get; set; }            // breakfast/lunch/dinner/snack
    public string? Advice { get; set; }              // Lời khuyên từ Gemini
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public ApplicationUser? ApplicationUser { get; set; }
    public List<PredictionDetail> Details { get; set; }
}
```

### 5.3 PredictionDetail (Sub-details)

```csharp
[Table("PredictionDetail")]
public class PredictionDetail
{
    [Key]
    public int Id { get; set; }
    public int PredictionHistoryId { get; set; }     // Foreign key
    public string Label { get; set; }                // "Phở", "Thịt", "Rau"
    public double Weight { get; set; }               // g
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Fat { get; set; }
    public double Carbs { get; set; }
    public double Confidence { get; set; }
    
    // Navigation
    public PredictionHistory? PredictionHistory { get; set; }
}
```

### 5.4 HealthPlan (User's Nutrition Plan)

```csharp
[Table("HealthPlan")]
public class HealthPlan
{
    [Key]
    public int Id { get; set; }
    [ForeignKey("User")]
    public string? UserId { get; set; }              // User reference
    public string BenhLy { get; set; }               // "Tiểu đường"
    public string PhacDoText { get; set; }           // Mô tả chi tiết
    public string DinhDuong { get; set; }            // "2180 kcal; 66g P; 72g F; 311g C"
    public string KhuyenNghiMonAn { get; set; }      // Recommended foods
    public string ThoiGianDieuTri { get; set; }      // Duration
    public DateTime NgayTao { get; set; }
    
    // Navigation
    public virtual ApplicationUser? User { get; set; }
}
```

### 5.5 FoodAnalysisViewModel

```csharp
public class FoodAnalysisViewModel
{
    public string UserId { get; set; }
    public HealthProfile HealthProfile { get; set; }
}
```

---

## 6. Services

### 6.1 NutritionService

**Location**: `Services/NutritionService.cs`

**Purpose**: Xử lý logic dinh dưỡng, gọi Gemini API

**Key Methods**:

```csharp
/// <summary>
/// Lấy lời khuyên từ Gemini về bữa ăn
/// </summary>
/// <param name="userId">ID user (để lấy health plan)</param>
/// <param name="mealType">breakfast/lunch/dinner/snack</param>
/// <param name="mealCalories">Calories của bữa ăn</param>
/// <param name="foodName">Tên món ăn</param>
/// <returns>Lời khuyên (string)</returns>
public async Task<string> GetMealAdviceAsync(
    string userId, 
    string mealType, 
    double mealCalories, 
    string foodName)
{
    // 1. Query HealthPlan từ DB
    // 2. Call AskGeminiAsync
    // 3. Return advice
}

/// <summary>
/// Gọi Gemini API để lấy phản hồi AI
/// </summary>
private async Task<string> AskGeminiAsync(
    string food, 
    string condition, 
    string nutritionInfo, 
    string mealType, 
    double mealCalories)
{
    // Build prompt
    // POST to Gemini
    // Parse response
    // Return text
}
```

**Configuration** (appsettings.json):

```json
{
  "GoogleGemini": {
    "ApiKey": "your-api-key-here"
  }
}
```

**Dependencies**:
- `IHttpClientFactory` - HTTP requests
- `IConfiguration` - API key
- `AppDbContext` - Query HealthPlan

---

### 6.2 FoodAnalysisController

**Location**: `Controllers/FoodAnalysisController.cs`

**Route**: `/api/FoodAnalysis`

**Key Methods**:

```csharp
[HttpPost("analyze")]
public async Task<IActionResult> AnalyzeImage(
    IFormFile image, 
    [FromForm] string userId, 
    [FromForm] string mealType)

[HttpGet("history/{userId}")]
public IActionResult GetHistory(string userId)

[HttpDelete("history/{id}")]
public async Task<IActionResult> DeleteHistory(int id)
```

**Error Handling**:
- Validation errors → 400 BadRequest
- Python API errors → 500 InternalServerError
- File operations → Try-catch với logging

---

## 7. Database Schema

### Table: PredictionHistory

| Column | Type | Notes |
|--------|------|-------|
| Id | int | PK, Identity |
| UserId | nvarchar(max) | FK → AspNetUsers |
| ImagePath | nvarchar(max) | Full URL to image |
| FoodName | nvarchar(max) | "Phở Bò" |
| Confidence | float | 0.0-1.0 |
| Calories | float | kcal |
| Protein | float | g |
| Fat | float | g |
| Carbs | float | g |
| MealType | nvarchar(50) | breakfast/lunch/dinner/snack |
| Advice | nvarchar(max) | Gemini response |
| CreatedAt | datetime2 | Timestamp |

**Indexes**:
- Clustered: Id (PK)
- Non-clustered: UserId, CreatedAt (for efficient queries)

### Table: PredictionDetail

| Column | Type | Notes |
|--------|------|-------|
| Id | int | PK, Identity |
| PredictionHistoryId | int | FK → PredictionHistory |
| Label | nvarchar(max) | "Phở", "Thịt Bò" |
| Weight | float | g |
| Calories | float | kcal |
| Protein | float | g |
| Fat | float | g |
| Carbs | float | g |
| Confidence | float | Detection confidence |

**Foreign Key**:
```sql
ALTER TABLE PredictionDetail 
ADD CONSTRAINT FK_PredictionDetail_PredictionHistory 
    FOREIGN KEY (PredictionHistoryId) 
    REFERENCES PredictionHistory(Id)
    ON DELETE CASCADE;
```

### Query Examples

**Lấy lịch sử phân tích của user**:
```sql
SELECT * FROM PredictionHistory 
WHERE UserId = @userId 
ORDER BY CreatedAt DESC;
```

**Lấy detail của một phân tích**:
```sql
SELECT * FROM PredictionDetail 
WHERE PredictionHistoryId = @id 
ORDER BY Weight DESC;
```

**Tổng calories trong ngày**:
```sql
SELECT SUM(Calories) FROM PredictionHistory 
WHERE UserId = @userId 
  AND CAST(CreatedAt AS DATE) = CAST(GETDATE() AS DATE);
```

---

## 8. Integration cho Flutter

### 8.1 Dart Models

```dart
import 'package:json_annotation/json_annotation.dart';

part 'food_analysis.g.dart';

@JsonSerializable()
class PredictionHistory {
  final int id;
  final String userId;
  final String imagePath;
  final String foodName;
  final double confidence;
  final double calories;
  final double protein;
  final double fat;
  final double carbs;
  final String? mealType;
  final String? advice;
  @JsonKey(name: 'createdAt')
  final DateTime createdAt;
  final List<PredictionDetail> details;

  PredictionHistory({
    required this.id,
    required this.userId,
    required this.imagePath,
    required this.foodName,
    required this.confidence,
    required this.calories,
    required this.protein,
    required this.fat,
    required this.carbs,
    this.mealType,
    this.advice,
    required this.createdAt,
    required this.details,
  });

  factory PredictionHistory.fromJson(Map<String, dynamic> json) =>
      _$PredictionHistoryFromJson(json);

  Map<String, dynamic> toJson() => _$PredictionHistoryToJson(this);
}

@JsonSerializable()
class PredictionDetail {
  final String label;
  final double weight;
  final double confidence;
  final double calories;
  final double protein;
  final double fat;
  final double carbs;

  PredictionDetail({
    required this.label,
    required this.weight,
    required this.confidence,
    required this.calories,
    required this.protein,
    required this.fat,
    required this.carbs,
  });

  factory PredictionDetail.fromJson(Map<String, dynamic> json) =>
      _$PredictionDetailFromJson(json);

  Map<String, dynamic> toJson() => _$PredictionDetailToJson(this);
}
```

### 8.2 API Service

```dart
import 'package:http/http.dart' as http;
import 'dart:convert';

class FoodAnalysisService {
  static const String baseUrl = 'https://your-domain.com/api/FoodAnalysis';
  
  /// Phân tích ảnh món ăn
  Future<PredictionHistory> analyzeFood({
    required File imageFile,
    required String userId,
    String? mealType,
    String? cookie,
  }) async {
    try {
      var request = http.MultipartRequest(
        'POST',
        Uri.parse('$baseUrl/analyze'),
      );

      // Add fields
      request.fields['userId'] = userId;
      if (mealType != null) {
        request.fields['mealType'] = mealType;
      }

      // Add image file
      request.files.add(
        await http.MultipartFile.fromPath(
          'image',
          imageFile.path,
          contentType: MediaType('image', 'jpeg'),
        ),
      );

      // Add auth cookie if available
      if (cookie != null) {
        request.headers['Cookie'] = cookie;
      }

      var streamedResponse = await request.send();
      var response = await http.Response.fromStream(streamedResponse);

      if (response.statusCode == 200) {
        final json = jsonDecode(response.body);
        return PredictionHistory.fromJson(json);
      } else {
        throw Exception('Failed to analyze: ${response.body}');
      }
    } catch (e) {
      throw Exception('Error analyzing food: $e');
    }
  }

  /// Lấy lịch sử phân tích
  Future<List<PredictionHistory>> getHistory({
    required String userId,
    String? cookie,
  }) async {
    try {
      final headers = <String, String>{};
      if (cookie != null) {
        headers['Cookie'] = cookie;
      }

      final response = await http.get(
        Uri.parse('$baseUrl/history/$userId'),
        headers: headers,
      );

      if (response.statusCode == 200) {
        final List<dynamic> jsonList = jsonDecode(response.body);
        return jsonList
            .map((json) => PredictionHistory.fromJson(json))
            .toList();
      } else {
        throw Exception('Failed to load history: ${response.body}');
      }
    } catch (e) {
      throw Exception('Error loading history: $e');
    }
  }

  /// Xóa một bản ghi phân tích
  Future<void> deleteHistory({
    required int id,
    String? cookie,
  }) async {
    try {
      final headers = <String, String>{};
      if (cookie != null) {
        headers['Cookie'] = cookie;
      }

      final response = await http.delete(
        Uri.parse('$baseUrl/history/$id'),
        headers: headers,
      );

      if (response.statusCode != 204) {
        throw Exception('Failed to delete: ${response.body}');
      }
    } catch (e) {
      throw Exception('Error deleting history: $e');
    }
  }
}
```

### 8.3 Usage Example

```dart
// Select image from gallery
final picker = ImagePicker();
final pickedFile = await picker.pickImage(source: ImageSource.gallery);

if (pickedFile != null) {
  final file = File(pickedFile.path);
  
  // Analyze
  final result = await FoodAnalysisService().analyzeFood(
    imageFile: file,
    userId: 'user-123',
    mealType: 'lunch',
  );
  
  // Show result
  showDialog(
    context: context,
    builder: (context) => AlertDialog(
      title: Text(result.foodName),
      content: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Image.network(result.imagePath),
          SizedBox(height: 16),
          Text('Calories: ${result.calories.toStringAsFixed(0)} kcal'),
          Text('Protein: ${result.protein.toStringAsFixed(1)}g'),
          Text('Fat: ${result.fat.toStringAsFixed(1)}g'),
          Text('Carbs: ${result.carbs.toStringAsFixed(1)}g'),
          SizedBox(height: 16),
          Text('Lời khuyên:'),
          Text(result.advice ?? 'Không có lời khuyên'),
        ],
      ),
    ),
  );
}

// Load history
final history = await FoodAnalysisService().getHistory(userId: 'user-123');
```

---

## 9. Troubleshooting

### Issue 1: Python API Connection Error

**Error**: `Python API error: Connection refused at http://127.0.0.1:5000/predict`

**Causes**:
- Python Flask server không chạy
- Port 5000 bị firewall block
- Server listening trên interface khác

**Solutions**:
```bash
# 1. Kiểm tra Python server có chạy không
netstat -ano | findstr :5000

# 2. Khởi động Python server
python app.py

# 3. Nếu port 5000 đã dùng, thay đổi port:
# Trong backend, sửa URL thành: http://127.0.0.1:5001/predict
# Trong Python, chạy: python app.py --port 5001

# 4. Nếu server ở máy khác, sửa IP:
# http://192.168.x.x:5000/predict
```

### Issue 2: Gemini API Error

**Error**: `AI không phản hồi hợp lệ` hoặc `AI tạm thời không khả dụng`

**Causes**:
- API key invalid
- Rate limit exceeded
- Network timeout

**Solutions**:
```csharp
// 1. Kiểm tra API key trong appsettings.json
{
  "GoogleGemini": {
    "ApiKey": "AIzaSy..." // Should not be empty
  }
}

// 2. Kiểm tra rate limit - thêm retry logic
int retries = 0;
while (retries < 3) {
    try {
        return await AskGeminiAsync(...);
    } catch (HttpRequestException ex) when (ex.StatusCode == 429) {
        await Task.Delay(2000 * (retries + 1));
        retries++;
    }
}

// 3. Set timeout
_httpClient.Timeout = TimeSpan.FromSeconds(30);
```

### Issue 3: File Upload Size Limit

**Error**: `413 Payload Too Large`

**Solution** (Program.cs):
```csharp
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100MB
});
```

### Issue 4: No Health Plan Found

**Error**: `Không tìm thấy phác đồ của người dùng`

**Causes**:
- User chưa tạo health plan
- User ID không match

**Solution**:
```csharp
// Tạo default health plan cho user
var defaultPlan = new HealthPlan
{
    UserId = userId,
    BenhLy = "khỏe mạnh",
    DinhDuong = "2000 kcal; 50g P; 70g F; 280g C",
    PhacDoText = "Chế độ ăn cân bằng",
    KhuyenNghiMonAn = "Rau quả, protein lean, whole grains",
    NgayTao = DateTime.Now
};
_context.HealthPlans.Add(defaultPlan);
await _context.SaveChangesAsync();
```

### Issue 5: Image File Not Saved

**Error**: `File not found after save`

**Causes**:
- wwwroot folder không tồn tại
- Permission denied
- Path không chính xác

**Solution**:
```csharp
// Đảm bảo folder tồn tại
var uploadsFolder = Path.Combine(
    Directory.GetCurrentDirectory(), 
    "wwwroot", 
    "uploads"
);

if (!Directory.Exists(uploadsFolder))
{
    Directory.CreateDirectory(uploadsFolder);
    Console.WriteLine($"✅ Created folder: {uploadsFolder}");
}

// Check permissions
var testFile = Path.Combine(uploadsFolder, "test.txt");
try
{
    System.IO.File.WriteAllText(testFile, "test");
    System.IO.File.Delete(testFile);
    Console.WriteLine("✅ Folder writable");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Permission error: {ex.Message}");
}
```

---

## 10. Performance Optimization

### Caching Strategy

```csharp
// Cache health plan (5 minutes)
var cacheKey = $"healthplan_{userId}";
if (!_cache.TryGetValue(cacheKey, out HealthPlan? plan))
{
    plan = await _context.HealthPlans
        .Where(x => x.UserId == userId)
        .OrderByDescending(x => x.NgayTao)
        .FirstOrDefaultAsync();
    
    _cache.Set(cacheKey, plan, TimeSpan.FromMinutes(5));
}
```

### Async Processing

```csharp
// Offload Gemini call to background job
_ = _geminiService.GetAdviceAsync(userId, mealType, calories, foodName)
    .ContinueWith(async task => {
        if (task.IsCompletedSuccessfully) {
            var advice = task.Result;
            // Update PredictionHistory with advice
            history.Advice = advice;
            await _context.SaveChangesAsync();
        }
    });
```

### Image Optimization

```dart
// Compress image before upload
final bytes = File(imageFile.path).readAsBytesSync();
final compressedBytes = await FlutterImageCompress.compressWithList(
  bytes,
  minHeight: 1024,
  minWidth: 1024,
  quality: 80,
  rotate: 0,
);
final compressedFile = File('${tempDir.path}/compressed.jpg')
  ..writeAsBytesSync(compressedBytes);

// Send compressed file
await analyzeFood(
  imageFile: compressedFile,
  userId: userId,
);
```

---

## 📚 References

- [Google Gemini API Docs](https://ai.google.dev/)
- [ASP.NET File Upload](https://docs.microsoft.com/aspnet/core/mvc/models/file-uploads)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Flutter Image Picker](https://pub.dev/packages/image_picker)

---

**Last Updated**: November 9, 2025  
**Author**: Backend Team  
**Status**: ✅ Production Ready
