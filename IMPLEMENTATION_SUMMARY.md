# Food Analysis API - Implementation Summary

## ✅ Completed Tasks

### 1. Database Schema Analysis
- Analyzed existing SQL tables in Hotel_Web database:
  - `[dbo].[PredictionHistory]` - 15 columns including UserId, ImagePath, FoodName, Calories, Protein, Fat, Carbs, MealType, Reason, Suitable, Suggestions, Advice, CreatedAt
  - `[dbo].[PredictionDetail]` - 9 columns for component breakdown (Label, Weight, Calories, Protein, Fat, Carbs, Confidence)

### 2. Created Models

#### PredictionDetail.cs
```csharp
[Table("PredictionDetail")]
public class PredictionDetail
{
    public int Id { get; set; }
    public int PredictionHistoryId { get; set; }
    [ForeignKey(nameof(PredictionHistoryId))]
    public virtual PredictionHistory PredictionHistory { get; set; }
    
    public string Label { get; set; }
    public float Weight { get; set; }
    public int Calories { get; set; }
    public float Protein { get; set; }
    public float Fat { get; set; }
    public float Carbs { get; set; }
    public float Confidence { get; set; }
}
```

#### PredictionHistory.cs (Updated)
- Table: `[dbo].[PredictionHistory]` (singular)
- Required fields: `UserId`, `ImagePath`, `FoodName` (using `required` modifier)
- Optional fields: `MealType`, `Reason`, `Suggestions`, `Advice` (nullable strings)
- Numeric fields: `Confidence`, `Calories`, `Protein`, `Fat`, `Carbs`
- Evaluation fields: `Suitable` (int?), `CreatedAt` (DateTime)
- Navigation: `public virtual ICollection<PredictionDetail>? Details`

### 3. Updated AppDbContext.cs
```csharp
public DbSet<PredictionHistory> PredictionHistories { get; set; }
public DbSet<PredictionDetail> PredictionDetails { get; set; }
```

### 4. Implemented FoodAnalysisController Endpoints

#### POST /api/FoodAnalysis/analyze
**8-Step Workflow:**
1. Validate input (UserId required, image validation, content-type check)
2. Save physical file to `wwwroot/uploads/` with GUID naming
3. Build public URL: `https://localhost:7xxx/uploads/{fileName}`
4. Call Python API: `POST http://127.0.0.1:5000/predict` with multipart form
5. Query HealthPlan (required, ordered by NgayTao DESC)
6. Generate advice & evaluate suitability
7. Save PredictionHistory + PredictionDetail records to database
8. Return response with complete analysis

**Request Format (multipart/form-data):**
```
POST /api/FoodAnalysis/analyze
Content-Type: multipart/form-data

- Image: [binary file]
- UserId: [string, required]
- MealType: [string, optional - breakfast|lunch|dinner|snack]
```

**Response Format:**
```json
{
  "id": 1,
  "userId": "user-123",
  "imagePath": "https://localhost:7xxx/uploads/guid.jpg",
  "foodName": "Cơm gà",
  "confidence": 0.95,
  "calories": 550,
  "protein": 28.5,
  "fat": 12.3,
  "carbs": 72.1,
  "mealType": "lunch",
  "advice": "✓ Bữa ăn này phù hợp với phác đồ của bạn...",
  "createdAt": "2025-11-11T10:55:00Z",
  "details": [
    {
      "label": "cơm",
      "weight": 200,
      "confidence": 0.98,
      "calories": 300,
      "protein": 6,
      "fat": 1,
      "carbs": 65
    },
    {
      "label": "gà nướng",
      "weight": 150,
      "confidence": 0.92,
      "calories": 250,
      "protein": 35,
      "fat": 8,
      "carbs": 0
    }
  ]
}
```

#### GET /api/FoodAnalysis/history/{userId}
- Returns array of user's analysis history ordered by CreatedAt DESC
- Returns: `[{ id, imagePath, foodName, confidence, calories, protein, fat, carbs, mealType, createdAt, advice }, ...]`

#### DELETE /api/FoodAnalysis/history/{id}
- Deletes a PredictionHistory record and all related PredictionDetail records
- Returns: 204 NoContent on success, 404 if not found

#### GET /api/FoodAnalysis/test-url/{filename}
- Test endpoint to verify file URL accessibility
- Returns: File existence status and path information

### 5. Helper Methods

#### `GenerateAdvice(foodName, calories, healthPlan)`
Generates AI-powered advice based on:
- Parsed calorie goal from `healthPlan.DinhDuong` (format: "2180 kcal; 66g P; 72g F; 311g C")
- Comparison against remaining daily calories
- Recommendation for approved foods from `healthPlan.KhuyenNghiMonAn`
- Returns: Vietnamese advice with ✓ or ⚠ emoji

#### `EvaluateSuitability(calories, healthPlan)`
Calculates percentage match (0-100%) based on:
- Meal calories vs. daily calorie goal
- Returns: Integer percentage (capped at 100)
- Stored in `Suitable` field

#### `GenerateSuggestions(foodName, suitabilityScore, healthPlan)`
Provides improvement suggestions based on suitability score:
- >90%: "Bữa ăn này rất phù hợp!..."
- 70-90%: "Bữa ăn này tương đối phù hợp..."
- 50-70%: "Bữa ăn này có chứa nhiều calo..."
- <50%: "Bữa ăn này không phù hợp..."

#### `CallPythonApiAsync(imagePath, fileName)`
Calls external Python AI service:
- Endpoint: `http://127.0.0.1:5000/predict`
- Request: Multipart form with image file
- Response: JSON with predicted_label, confidence, nutrition (calories, protein, fat, carbs), details array
- Error handling: Catches HttpRequestException and returns 500 with error message

### 6. Data Flow

```
User Request
  ↓
[FoodAnalysisController.AnalyzeFood]
  ├─ Validate Input
  ├─ Save Image File → wwwroot/uploads/
  ├─ Call Python API ← http://127.0.0.1:5000/predict
  ├─ Query HealthPlan ← Database (required)
  ├─ Generate Advice & Score
  ├─ Save PredictionHistory ← Database
  ├─ Save PredictionDetail records ← Database
  └─ Return 200 OK with Analysis
```

### 7. Database Relationships

```
ApplicationUser (1) ──→ (N) PredictionHistory
                              ↓ (1)
                              (N)
                        PredictionDetail
```

- PredictionHistory.UserId → ApplicationUser.Id (FK)
- PredictionDetail.PredictionHistoryId → PredictionHistory.Id (FK, cascade delete)
- PredictionHistory has virtual collection `Details` for eager loading

### 8. Build Status

**✅ Build Successful**
- 0 Errors
- 3 Warnings (non-blocking nullable reference warnings)
- Build time: 2.66s

### 9. Server Status

**✅ Running Successfully**
- HTTPS: `https://localhost:7135`
- HTTP: `http://localhost:5217`
- Swagger: `https://localhost:7135/swagger/index.html`

### 10. File Structure

```
Hotel_API/
├── Controllers/
│   └── FoodAnalysisController.cs [UPDATED - 466 lines]
├── Models/
│   ├── PredictionHistory.cs [UPDATED - 98 lines]
│   └── PredictionDetail.cs [NEW - 56 lines]
├── Data/
│   └── AppDbContext.cs [UPDATED - added DbSet<PredictionDetail>]
├── Models/ViewModels/
│   └── FoodAnalysisFormDto.cs [EXISTING - used for form binding]
└── Services/
    └── MediaUrlService.cs [EXISTING - for URL generation]
```

## 🔧 Configuration

### AppSettings (appsettings.json)
```json
{
  "ConnectionStrings": {
    "HotelWebConnection": "Data Source=DESKTOP-YURI\\SQLEXPRESS;Initial Catalog=Hotel_Web;..."
  }
}
```

### Launch Settings (Properties/launchSettings.json)
- HTTPS Port: 7135
- HTTP Port: 5217
- Environment: Development

### Python API Configuration
- Endpoint: `http://127.0.0.1:5000/predict`
- Expected to be running on localhost
- Timeout: Default HttpClient timeout (100 seconds)

## 🚀 Testing

### Prerequisites
1. ✅ SQL Server with Hotel_Web database running
2. ✅ Python AI service running on http://127.0.0.1:5000
3. ✅ HealthPlan records exist for test user
4. ✅ ASP.NET Core runtime 9.0

### Test Scenario
```bash
# Start API
cd Hotel_API
dotnet run

# Test POST /api/FoodAnalysis/analyze
curl -X POST "https://localhost:7135/api/FoodAnalysis/analyze" \
  -F "image=@/path/to/food.jpg" \
  -F "userId=user-123" \
  -F "mealType=lunch"

# Expected: 200 OK with analysis response
```

## ⚠️ Important Notes

1. **Table Name**: Uses `[dbo].[PredictionHistory]` (singular) - NOT `PredictionHistories`
   - The DbSet is `PredictionHistories` but table attribute is `[Table("PredictionHistory")]`

2. **Health Plan Required**: User must have at least one HealthPlan record
   - Query: `SELECT * FROM HealthPlan WHERE UserId = @userId ORDER BY NgayTao DESC`
   - Returns: 400 BadRequest if none found

3. **Python API Required**: Image analysis depends on external Python service
   - If service is down: Returns 500 with connection error
   - Expected response format must include: `predicted_label`, `confidence`, `nutrition`, `details`

4. **File Storage**: Images saved to `wwwroot/uploads/` with GUID filenames
   - Public URLs: `https://localhost:7xxx/uploads/{guid}.jpg`
   - File extension preserved from original upload

5. **Nullable Fields**: Several fields are nullable to match database schema
   - Required: UserId, ImagePath, FoodName
   - Optional: MealType, Reason, Suggestions, Advice

6. **Error Handling**: All exceptions caught and returned as 500 with detail message
   - Includes Python API errors
   - Includes database constraint violations
   - Includes file I/O errors

## 📝 Recent Changes

- Changed table reference from `PredictionHistories` to `PredictionHistory`
- Added `Reason`, `Suitable`, `Suggestions` fields to match database
- Implemented `PredictionDetail` model for component breakdown
- Added helper methods for advice generation and suitability evaluation
- Implemented comprehensive error handling with descriptive messages
- Added database save for PredictionDetail records with cascade relationships

## ✨ Features Implemented

✅ Multipart form-data image upload
✅ Image file persistence to disk
✅ Python AI integration for food recognition
✅ Health plan validation (mandatory)
✅ Nutritional analysis and advice generation
✅ Suitability scoring (0-100%)
✅ Component-level breakdown (PredictionDetail)
✅ Database persistence with relationships
✅ Comprehensive error handling
✅ Vietnamese localization for messages
✅ History retrieval by user
✅ History deletion
✅ Test endpoints

