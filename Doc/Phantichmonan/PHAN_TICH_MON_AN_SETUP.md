# 🔧 Phân Tích Món Ăn - Setup & Testing Guide

> Hướng dẫn setup hệ thống Food Analysis và testing

---

## 📋 Mục Lục

1. [Prerequisites](#1-prerequisites)
2. [Backend Setup](#2-backend-setup)
3. [Python Service Setup](#3-python-service-setup)
4. [Gemini API Setup](#4-gemini-api-setup)
5. [Testing](#5-testing)
6. [Deployment](#6-deployment)

---

## 1. Prerequisites

### Requirements

- **Backend**: ASP.NET Core 6/7 SDK
- **Python**: 3.8+
- **Database**: SQL Server 2019+
- **API Keys**: Google Gemini API key
- **Tools**: Postman, Visual Studio Code

### Installation

```bash
# 1. Install .NET SDK
# Visit: https://dotnet.microsoft.com/download

# 2. Install Python
# Visit: https://www.python.org/downloads
python --version

# 3. Verify SQL Server
sqlcmd -S your-server-name -U sa -P your-password
```

---

## 2. Backend Setup

### 2.1 Configure appsettings

**File**: `appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "HotelWebConnection": "Server=DESKTOP-YURI\\SQLEXPRESS;Database=Hotel_Web;Trusted_Connection=True;"
  },
  "GoogleGemini": {
    "ApiKey": "AIzaSy... (your API key here)"
  }
}
```

### 2.2 Database Migration

```powershell
# In Package Manager Console
cd Hotel_Web

# Add migration for Food Analysis tables
Add-Migration AddFoodAnalysisTables

# Update database
Update-Database
```

**Migration Script** (if needed manually):

```sql
-- Create PredictionHistory table
CREATE TABLE [PredictionHistory] (
    [Id] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [UserId] NVARCHAR(MAX) NOT NULL,
    [ImagePath] NVARCHAR(MAX) NOT NULL,
    [FoodName] NVARCHAR(MAX) NOT NULL,
    [Confidence] FLOAT NOT NULL,
    [Calories] FLOAT NOT NULL,
    [Protein] FLOAT NOT NULL,
    [Fat] FLOAT NOT NULL,
    [Carbs] FLOAT NOT NULL,
    [MealType] NVARCHAR(50),
    [Advice] NVARCHAR(MAX),
    [CreatedAt] DATETIME2 NOT NULL,
    FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id])
);

-- Create PredictionDetail table
CREATE TABLE [PredictionDetail] (
    [Id] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [PredictionHistoryId] INT NOT NULL,
    [Label] NVARCHAR(MAX) NOT NULL,
    [Weight] FLOAT NOT NULL,
    [Calories] FLOAT NOT NULL,
    [Protein] FLOAT NOT NULL,
    [Fat] FLOAT NOT NULL,
    [Carbs] FLOAT NOT NULL,
    [Confidence] FLOAT NOT NULL,
    FOREIGN KEY ([PredictionHistoryId]) REFERENCES [PredictionHistory]([Id]) ON DELETE CASCADE
);

-- Create indexes
CREATE NONCLUSTERED INDEX [IX_PredictionHistory_UserId] 
    ON [PredictionHistory]([UserId]);
    
CREATE NONCLUSTERED INDEX [IX_PredictionHistory_CreatedAt] 
    ON [PredictionHistory]([CreatedAt] DESC);
```

### 2.3 Create Upload Folder

```csharp
// Program.cs - add after app build

// Ensure upload folder exists
var uploadsPath = Path.Combine(app.Environment.WebRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
    Console.WriteLine($"✅ Created uploads folder: {uploadsPath}");
}
```

Or manually:

```bash
# Create folder
mkdir wwwroot\uploads

# Verify
dir wwwroot\uploads
```

### 2.4 Verify Services Registration

**File**: `Program.cs`

```csharp
// Add to services
builder.Services.AddScoped<NutritionService>();
builder.Services.AddScoped<FoodAnalysisController>();

// Add HttpClientFactory
builder.Services.AddHttpClient<NutritionService>();

// Add form size limit
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100MB
});
```

### 2.5 Test Backend

```bash
# Build
dotnet build

# Run
dotnet run

# Expected output:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: https://localhost:7xxx
```

---

## 3. Python Service Setup

### 3.1 Create Virtual Environment

```bash
# 1. Create venv
python -m venv venv

# 2. Activate
# On Windows:
venv\Scripts\activate

# On Mac/Linux:
source venv/bin/activate
```

### 3.2 Install Dependencies

**File**: `requirements.txt`

```
Flask==2.3.0
python-dotenv==1.0.0
opencv-python==4.6.0
numpy==1.24.0
tensorflow==2.13.0
# Add other ML model dependencies
```

**Install**:

```bash
pip install -r requirements.txt
```

### 3.3 Create Flask App

**File**: `app.py`

```python
from flask import Flask, request, jsonify
from werkzeug.utils import secure_filename
import json
import os
from datetime import datetime
# Import your ML model here
# from model import predict_food

app = Flask(__name__)

# Config
UPLOAD_FOLDER = 'uploads'
ALLOWED_EXTENSIONS = {'jpg', 'jpeg', 'png', 'gif'}
os.makedirs(UPLOAD_FOLDER, exist_ok=True)

def allowed_file(filename):
    return '.' in filename and filename.rsplit('.', 1)[1].lower() in ALLOWED_EXTENSIONS

@app.route('/predict', methods=['POST'])
def predict():
    """
    Expected response:
    {
        "predicted_label": "Phở Bò",
        "confidence": 0.95,
        "nutrition": {
            "total_weight": 500,
            "calories": 450,
            "fat": 12,
            "carbs": 60,
            "protein": 25,
            "mealType": null
        },
        "details": [
            {
                "label": "Phở",
                "confidence": 0.92,
                "weight": 300,
                "cal": 280,
                "fat": 5,
                "carbs": 48,
                "protein": 12
            }
        ]
    }
    """
    
    if 'file' not in request.files:
        return jsonify({'error': 'No file provided'}), 400
    
    file = request.files['file']
    
    if file.filename == '':
        return jsonify({'error': 'No file selected'}), 400
    
    if not allowed_file(file.filename):
        return jsonify({'error': 'Invalid file type'}), 400
    
    try:
        # Save file
        filename = secure_filename(file.filename)
        filepath = os.path.join(UPLOAD_FOLDER, filename)
        file.save(filepath)
        
        # Predict (replace with your model)
        # result = predict_food(filepath)
        
        # Mock result for testing
        result = {
            "predicted_label": "Phở Bò",
            "confidence": 0.95,
            "nutrition": {
                "total_weight": 500,
                "calories": 450,
                "fat": 12,
                "carbs": 60,
                "protein": 25,
                "mealType": None
            },
            "details": [
                {
                    "label": "Phở",
                    "confidence": 0.92,
                    "weight": 300,
                    "cal": 280,
                    "fat": 5,
                    "carbs": 48,
                    "protein": 12
                },
                {
                    "label": "Thịt Bò",
                    "confidence": 0.89,
                    "weight": 100,
                    "cal": 140,
                    "fat": 5,
                    "carbs": 0,
                    "protein": 22
                },
                {
                    "label": "Rau Thơm",
                    "confidence": 0.85,
                    "weight": 20,
                    "cal": 30,
                    "fat": 0.5,
                    "carbs": 6,
                    "protein": 1
                }
            ]
        }
        
        return jsonify(result), 200
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/health', methods=['GET'])
def health():
    return jsonify({'status': 'OK', 'timestamp': datetime.now().isoformat()}), 200

if __name__ == '__main__':
    app.run(host='127.0.0.1', port=5000, debug=True)
```

### 3.4 Run Python Service

```bash
# Run Flask
python app.py

# Expected output:
# WARNING in app.run()
#   Use a production WSGI server instead.
# Running on http://127.0.0.1:5000
# Press CTRL+C to quit
```

---

## 4. Gemini API Setup

### 4.1 Get API Key

1. Visit [Google AI Studio](https://aistudio.google.com/app/apikey)
2. Click "Get API key"
3. Create new project
4. Create API key
5. Copy key

### 4.2 Configure

**File**: `appsettings.Development.json`

```json
{
  "GoogleGemini": {
    "ApiKey": "AIzaSy..." // Paste your key here
  }
}
```

### 4.3 Test Gemini Connection

```csharp
// Add to a test controller
[HttpGet("test-gemini")]
public async Task<IActionResult> TestGemini()
{
    var service = new NutritionService(_context, _httpClientFactory, _configuration);
    
    try
    {
        var advice = await service.GetMealAdviceAsync(
            userId: "test-user",
            mealType: "lunch",
            mealCalories: 450,
            foodName: "Phở Bò"
        );
        
        return Ok(new { message = advice });
    }
    catch (Exception ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}
```

---

## 5. Testing

### 5.1 Setup Test Data

**Create Health Plan**:

```csharp
// In seed data or test endpoint
var healthPlan = new HealthPlan
{
    UserId = "test-user-id",
    BenhLy = "khỏe mạnh",
    DinhDuong = "2180 kcal; 66g P; 72g F; 311g C",
    PhacDoText = "Chế độ ăn cân bằng cho người khỏe mạnh",
    KhuyenNghiMonAn = "Rau quả, protein lean, whole grains",
    ThoiGianDieuTri = "Hàng ngày",
    NgayTao = DateTime.Now
};

_context.HealthPlans.Add(healthPlan);
await _context.SaveChangesAsync();
```

### 5.2 Postman Collection

**1. Create Environment**:

```json
{
  "name": "Hotel Web - Food Analysis",
  "values": [
    {
      "key": "base_url",
      "value": "https://localhost:7xxx"
    },
    {
      "key": "api_url",
      "value": "{{base_url}}/api/FoodAnalysis"
    },
    {
      "key": "user_id",
      "value": "test-user-id"
    }
  ]
}
```

**2. Create Requests**:

#### Analyze Image

```
POST {{api_url}}/analyze
Content-Type: multipart/form-data

[Form Data]
image: [Select binary file]
userId: {{user_id}}
mealType: lunch
```

#### Get History

```
GET {{api_url}}/history/{{user_id}}
```

#### Delete History

```
DELETE {{api_url}}/history/1
```

#### Test Python Connection

```
POST http://127.0.0.1:5000/predict
Content-Type: multipart/form-data

[Form Data]
file: [Select binary file]
```

### 5.3 Manual Testing

**Test 1: Image Upload**

```bash
# Using curl
curl -X POST http://localhost:7xxx/api/FoodAnalysis/analyze \
  -F "image=@test.jpg" \
  -F "userId=test-user" \
  -F "mealType=lunch"
```

**Test 2: History**

```bash
curl http://localhost:7xxx/api/FoodAnalysis/history/test-user
```

**Test 3: Check File Saved**

```bash
# Verify file exists
dir wwwroot\uploads\
```

---

## 6. Deployment

### 6.1 Production Configuration

**File**: `appsettings.json`

```json
{
  "ConnectionStrings": {
    "HotelWebConnection": "Server=prod-server;Database=Hotel_Web;User Id=sa;Password=***;"
  },
  "GoogleGemini": {
    "ApiKey": "your-production-key"
  }
}
```

### 6.2 Publish ASP.NET

```bash
# Build release
dotnet publish -c Release -o bin/publish

# Or use Visual Studio
# Right-click project → Publish
```

### 6.3 Deploy Python Service

```bash
# Using Gunicorn (production WSGI)
pip install gunicorn

# Run
gunicorn -w 4 -b 127.0.0.1:5000 app:app

# Or systemd service (Linux)
# Create /etc/systemd/system/food-analysis.service
```

**Service File** (Linux):

```ini
[Unit]
Description=Food Analysis Python Service
After=network.target

[Service]
Type=simple
User=www-data
WorkingDirectory=/path/to/app
ExecStart=/path/to/venv/bin/gunicorn -w 4 -b 127.0.0.1:5000 app:app
Restart=always

[Install]
WantedBy=multi-user.target
```

### 6.4 Docker Setup (Optional)

**Dockerfile**:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /src
COPY Hotel_Web.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app .

EXPOSE 80
ENTRYPOINT ["dotnet", "Hotel_Web.dll"]
```

**docker-compose.yml**:

```yaml
version: '3.8'

services:
  backend:
    build: .
    ports:
      - "7043:80"
    environment:
      - ConnectionStrings__HotelWebConnection=Server=db;Database=Hotel_Web;User Id=sa;Password=YourPassword123!
      - GoogleGemini__ApiKey=your-api-key
    depends_on:
      - db

  python:
    build: ./python_service
    ports:
      - "5000:5000"

  db:
    image: mcr.microsoft.com/mssql/server:2019-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword123!
    ports:
      - "1433:1433"
```

---

## ✅ Checklist

- [ ] .NET SDK installed
- [ ] Python 3.8+ installed
- [ ] SQL Server running
- [ ] Database migrated
- [ ] Uploads folder created
- [ ] Gemini API key obtained
- [ ] appsettings configured
- [ ] NutritionService registered
- [ ] Python venv created
- [ ] Flask app running on port 5000
- [ ] Test endpoints in Postman
- [ ] Health plan created for test user
- [ ] Image upload tested
- [ ] History retrieval tested
- [ ] Delete functionality tested

---

## 🐛 Troubleshooting

### Backend Issues

```
Error: "Không tìm thấy phác đồ"
→ Create health plan for user first

Error: "Python API error"
→ Check if Flask running on port 5000

Error: "File not found"
→ Verify wwwroot/uploads folder exists
```

### Python Issues

```
ModuleNotFoundError: No module named 'flask'
→ pip install flask

ConnectionError: Connection refused
→ Check firewall allows port 5000
```

### Gemini Issues

```
403 Forbidden
→ Check API key is valid

429 Too Many Requests
→ Implement retry with exponential backoff
```

---

## 📞 Support

- Check logs in `wwwroot/logs/`
- Debug Python with `app.py --debug`
- Enable detailed errors: `DetailedErrors = true` in appsettings

---

**Last Updated**: November 9, 2025
