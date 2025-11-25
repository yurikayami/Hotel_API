# API Fallback Architecture - Python to Gemini

## 📋 Overview

Khi Python AI không nhận diện được thực phẩm (confidence = 0 hoặc foodName = "Không xác định"), hệ thống sẽ **tự động fallback** sang Gemini Vision API để phân tích lại. Client Flutter không cần biết về fallback này - API sẽ luôn trả về kết quả hợp lệ.

## 🔄 Analysis Flow

```
Image Upload
    ↓
Step 1: Save image to server
    ↓
Step 2: Try Python API
    ↓
    ├─ SUCCESS (confidence > 0) → Use Python results
    │
    └─ FAIL (confidence = 0 or "Không xác định") → Fallback to Gemini
        ↓
        Step 3: Call Gemini Vision API
        ↓
        Use Gemini results
    ↓
Step 4: Extract nutrition data
    ↓
Step 5: Get user's health plan
    ↓
Step 6: Generate advice based on health plan
    ↓
Step 7: Save to database
    ↓
Step 8: Return complete response
```

## 🔧 Components

### 1. GeminiService (New)
**File:** `Services/GeminiService.cs`

Handles Gemini Vision API calls:
```csharp
public async Task<GeminiAnalysisResult> AnalyzeFoodImageAsync(string imagePath)
{
    // Convert image to base64
    // Send to Gemini API
    // Parse JSON response
    // Return GeminiAnalysisResult
}
```

**Return Data:**
```csharp
public class GeminiAnalysisResult
{
    public string FoodName { get; set; }           // "Cơm Gà Hainan"
    public string Description { get; set; }        // "Cơm gà cổ điển"
    public double Confidence { get; set; }         // 0.85
    public double Calories { get; set; }           // 350
    public double Protein { get; set; }            // 25
    public double Fat { get; set; }                // 12
    public double Carbs { get; set; }              // 35
    public string? ServingSize { get; set; }       // "100g"
}
```

### 2. FoodAnalysisController (Updated)
**File:** `Controllers/FoodAnalysisController.cs`

Updated method `AnalyzeFood()`:
```csharp
[HttpPost("analyze")]
public async Task<ActionResult> AnalyzeFood([FromForm] FoodAnalysisFormDto model)
{
    // Step 1-3: Upload image, try Python API
    
    // Step 4.5: Check if Python failed, fallback to Gemini
    if (isPythonSuccess)
    {
        // Use Python results
        mainDish = pythonPrediction.predicted_label;
        calories = pythonPrediction.nutrition.calories;
        // ... etc
    }
    else
    {
        // Fallback to Gemini
        var geminiResult = await _geminiService.AnalyzeFoodImageAsync(filePath);
        mainDish = geminiResult.FoodName;
        calories = geminiResult.Calories;
        // ... etc
    }
    
    // Steps 5-8: Generate advice, save DB, return response
}
```

**Response (Always):**
```json
{
  "id": 123,
  "userId": "user-id",
  "imagePath": "https://...",
  "foodName": "Cơm Gà Hainan",          // Either Python or Gemini
  "confidence": 0.85,                   // Either Python or Gemini
  "calories": 350,
  "protein": 25,
  "fat": 12,
  "carbs": 35,
  "mealType": "lunch",
  "advice": "✓ Bữa ăn...",
  "createdAt": "2025-11-24T10:30:00Z"
}
```

## ⚙️ Configuration

### Add to appsettings.json:
```json
{
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY"
  }
}
```

### Get Gemini API Key:
1. Go to https://ai.google.dev/
2. Click "Get API Key"
3. Create new API key for "API (Gemini)" 
4. Copy and paste to appsettings.json

### Register in Program.cs (Already Done):
```csharp
// Services
builder.Services.AddScoped<GeminiService>();

// HttpClient for Gemini
builder.Services.AddHttpClient();
```

## 📊 Detection Logic

### When to Fallback:

```csharp
bool isPythonSuccess = false;

if (pythonConfidence == 0 || 
    pythonMainDish == "Không xác định" || 
    string.IsNullOrEmpty(pythonMainDish))
{
    // Python failed → use Gemini
    isPythonSuccess = false;
}
else
{
    // Python succeeded → use Python
    isPythonSuccess = true;
}
```

### Failure Cases:
1. **Python API throws exception** → Catch block sets `isPythonSuccess = false`
2. **Python returns confidence = 0** → Detected and fallback
3. **Python returns "Không xác định"** → Detected and fallback
4. **Python returns null/empty name** → Detected and fallback

## 🎯 Benefits

| Scenario | Before | After |
|----------|--------|-------|
| Python recognizes food | ✅ Python result | ✅ Python result (Fast) |
| Python fails | ❌ Empty/null result | ✅ Gemini result (Fallback) |
| Python crashes | ❌ Error response | ✅ Gemini result (Reliable) |
| Unclear food image | ❌ Wrong result | ✅ Gemini handles better (Smart) |

## 📝 Logging

All events are logged to help debug:

```
// Python succeeds
Using Python analysis: Cơm Gà

// Python fails, switching to Gemini  
Python API failed to recognize food (confidence: 0). Using Gemini as fallback.
Switching to Gemini API for food analysis
Using Gemini analysis: Cơm Gà (confidence: 0.85)

// Python crashes, switching to Gemini
Python API error: Connection refused. Using Gemini as fallback.
Switching to Gemini API for food analysis
Using Gemini analysis: Cơm Gà (confidence: 0.85)
```

## 🧪 Testing

### Test 1: Recognizable Dish (Python Success)
```
Request: Food image (clear, recognizable)
Expected: 
  - Python API called first
  - Returns quickly
  - Confidence > 0
  - FoodName populated
  - Log: "Using Python analysis: ..."
```

### Test 2: Unrecognizable Image (Python Fails)
```
Request: Food image (blurry, unclear)
Expected:
  - Python API called first
  - Returns confidence = 0
  - Gemini API called as fallback
  - Returns valid result
  - Log: "Python API failed... Using Gemini"
  - Log: "Using Gemini analysis: ..."
```

### Test 3: Python API Offline (Exception)
```
Request: Any food image (Python server down)
Expected:
  - Python API call throws exception
  - Gemini API called automatically
  - Returns valid result
  - Log: "Python API error: ... Using Gemini as fallback"
```

### Test 4: Response Consistency
```
Request 1: Via Python → {"foodName": "Cơm Gà", "confidence": 0.95}
Request 2: Via Gemini → {"foodName": "Cơm Gà", "confidence": 0.85}
Expected: Both return same foodName (even if confidence differs)
```

## 🔐 Error Handling

### Gemini API Errors:
```csharp
try
{
    var geminiResult = await _geminiService.AnalyzeFoodImageAsync(filePath);
    // If Gemini also fails, exception will bubble up to outer catch
}
catch (Exception ex)
{
    // Both Python and Gemini failed
    return StatusCode(500, new
    {
        error = "Food analysis failed",
        message = ex.Message
    });
}
```

### If Both Fail:
- Log: "Error: Both Python and Gemini failed"
- Response: 500 error with error message
- No partial/invalid data returned

## 📌 Important Notes

1. **Frontend doesn't change**: Flutter code continues working exactly as before
2. **Transparent fallback**: Client doesn't need to handle fallback logic
3. **Consistent response**: Response always has foodName, confidence, nutrition data
4. **Database saved**: Record saved with correct food name regardless of source
5. **Health plan applies**: Advice generated based on fallback result too

## 🚀 Future Improvements

1. **Caching**: Cache Gemini results for same images
2. **Hybrid model**: Combine Python + Gemini for higher confidence
3. **User feedback**: Let users correct AI recognition for retraining
4. **Confidence threshold**: Fallback when Python confidence < threshold (e.g., 0.5)
5. **Async queue**: Queue fallback requests to not block main thread

## 📞 Troubleshooting

### Issue: Gemini API returns 401
**Cause:** Invalid or missing API key
**Solution:** Check `appsettings.json` Gemini:ApiKey is set correctly

### Issue: Gemini returns "Không xác định"
**Cause:** Image is too blurry or not a food image
**Solution:** Client should prompt user to take better photo

### Issue: Response takes 30+ seconds
**Cause:** Both Python and Gemini processing
**Solution:** This is expected for fallback scenarios. Python is fast (5-10s), Gemini slower (10-20s)

---

**Status:** ✅ Implemented and Ready for Testing  
**Last Updated:** November 24, 2025
