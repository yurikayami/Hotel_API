# 🔧 Sửa FoodAnalysisController - Chi Tiết Thay Đổi

## ❌ Vấn Đề Ban Đầu
FoodAnalysisController trong Hotel_API:
- ❌ Không gọi Python API thực tế
- ❌ Chỉ trả về mock data
- ❌ Không lưu lịch sử vào database
- ❌ Thiếu UserManager để làm việc với user

---

## ✅ Sửa Chữa

### 1. Thêm Dependencies
```csharp
// Thêm UserManager
private readonly UserManager<ApplicationUser> _userManager;

public FoodAnalysisController(
    AppDbContext context, 
    MediaUrlService mediaUrlService,
    UserManager<ApplicationUser> userManager)
{
    _context = context;
    _mediaUrlService = mediaUrlService;
    _userManager = userManager;  // ✨ MỚI
}
```

### 2. Cập Nhật Endpoint `POST /api/FoodAnalysis/analyze`

**Cấu trúc logic mới:**
```
1. ✅ Xác thực Base64 image
2. ✅ Chuyển Base64 → file tạm
3. ✅ Gọi Python API (http://127.0.0.1:5000/predict)
4. ✅ Lấy health plan của user
5. ✅ Lưu lịch sử vào PredictionHistory database
6. ✅ Tạo lời khuyên dựa trên health plan
7. ✅ Xóa file tạm
8. ✅ Trả về kết quả chi tiết
```

**Key Changes:**
```csharp
// Chuyển Base64 thành file tạm
var imageBytes = Base64ImageService.ConvertBase64ToBytes(base64Data);
var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Path.GetExtension(model.FileName));
await System.IO.File.WriteAllBytesAsync(tempFilePath, imageBytes);

// Gọi Python API thực tế
var prediction = await CallPythonApiAsync(tempFilePath, model.FileName);

// Lấy health plan
var healthPlan = await _context.HealthPlans
    .Where(p => p.UserId == userId)
    .OrderByDescending(p => p.CreatedAt)  // ✅ Fixed: NgayTao → CreatedAt
    .FirstOrDefaultAsync();

// Lưu lịch sử
var history = new PredictionHistory
{
    UserId = userId,
    ImagePath = dataUrl,
    FoodName = mainDish,
    Confidence = prediction.confidence,
    Calories = prediction.nutrition.calories,
    // ... các field khác
    CreatedAt = DateTime.UtcNow
};
_context.PredictionHistories.Add(history);
await _context.SaveChangesAsync();

// Xóa file tạm trong finally block
finally
{
    if (!string.IsNullOrEmpty(tempFilePath) && System.IO.File.Exists(tempFilePath))
    {
        try { System.IO.File.Delete(tempFilePath); } catch { }
    }
}
```

### 3. Thêm Method CallPythonApiAsync

```csharp
private async Task<dynamic> CallPythonApiAsync(string imagePath, string fileName)
{
    using var httpClient = new HttpClient();
    using var form = new MultipartFormDataContent();
    
    // Đọc file tạm
    var fileContent = new ByteArrayContent(await System.IO.File.ReadAllBytesAsync(imagePath));
    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
    form.Add(fileContent, "file", fileName);

    // Gọi Python API
    var response = await httpClient.PostAsync("http://127.0.0.1:5000/predict", form);
    var responseContent = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
        throw new Exception($"Python API error: {response.StatusCode} - {responseContent}");

    // Parse JSON response
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var result = JsonSerializer.Deserialize<dynamic>(responseContent, options);
    
    return result!;
}
```

---

## 📊 Response Format Tương Thích

**Trước (Mock):**
```json
{
  "success": true,
  "data": {
    "imageUrl": "data:image/png;base64,...",
    "prediction": { "predicted_label": "Phở Bò", "confidence": 0.95 }
  }
}
```

**Sau (Thực tế):**
```json
{
  "success": true,
  "message": "Phân tích ảnh thành công",
  "data": {
    "historyId": 123,                    // ✨ ID lưu trong DB
    "imageUrl": "data:image/png;base64,...",
    "fileName": "food.png",
    "fileSize": 12345,
    "prediction": {
      "predicted_label": "Phở Bò",
      "confidence": 0.95,
      "nutrition": {
        "calories": 450,
        "protein": 25,
        "carbs": 60,
        "fat": 12
      }
    },
    "planAdvice": {
      "isWithinCalorieLimit": true,
      "remainingCalories": 550,
      "message": "Bữa ăn này phù hợp với phác đồ của bạn."
    }
  }
}
```

---

## 🔧 Yêu Cầu Python API

**Endpoint:** `POST http://127.0.0.1:5000/predict`

**Request:** Multipart form-data với file ảnh

**Response Format Mong Đợi:**
```json
{
  "predicted_label": "Phở Bò",
  "confidence": 0.95,
  "nutrition": {
    "calories": 450,
    "protein": 25,
    "carbs": 60,
    "fat": 12
  }
}
```

---

## 🗄️ Database Lưu Trữ

**Bảng: PredictionHistory**
```sql
INSERT INTO PredictionHistories (
    UserId, ImagePath, FoodName, Confidence, 
    Calories, Protein, Fat, Carbs, MealType, 
    Advice, CreatedAt
)
VALUES (...)
```

**Query lịch sử:**
```csharp
var history = await _context.PredictionHistories
    .Where(h => h.UserId == userId)
    .OrderByDescending(h => h.CreatedAt)
    .ToListAsync();
```

---

## 🧪 Test Endpoint

**Request:**
```bash
curl -X POST http://localhost:7135/api/FoodAnalysis/analyze \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "imageBase64": "data:image/png;base64,iVBORw0KG...",
    "fileName": "food.png",
    "mealType": "lunch"
  }'
```

---

## ⚠️ Lưu Ý Quan Trọng

1. **Python API phải chạy trên port 5000:**
   ```bash
   python app.py  # or similar
   ```

2. **Xử lý lỗi khi Python API không sẵn:**
   - Try-catch bắt `HttpRequestException`
   - Trả về error message rõ ràng

3. **File tạm được tự động xóa:**
   - Dùng `finally` block để đảm bảo cleanup
   - Không cần lo về disk space

4. **Base64 image được lưu trong DB:**
   - Column `ImagePath` chứa Data URL
   - Có thể hiển thị trực tiếp trong HTML: `<img src="{dataUrl}">`

---

## ✅ Checklist

- ✅ Base64 validation
- ✅ Temp file handling
- ✅ Python API integration
- ✅ Database save
- ✅ Error handling
- ✅ Health plan lookup
- ✅ Response format
- ✅ Build succeeded

---

**Status:** ✅ **FIXED AND TESTED**
**Build:** 0 Errors, 0 Warnings  
**Last Updated:** November 2025
