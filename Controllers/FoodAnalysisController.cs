using Hotel_API.Data;
using Hotel_API.Models;
using Hotel_API.Models.ViewModels;
using Hotel_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using System.Net.Http.Headers;

namespace Hotel_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FoodAnalysisController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly MediaUrlService _mediaUrlService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly GeminiService _geminiService;
        private readonly ILogger<FoodAnalysisController> _logger;
        private readonly TimeZoneInfo _vietnamTimeZone;

        public FoodAnalysisController(
            AppDbContext context,
            MediaUrlService mediaUrlService,
            UserManager<ApplicationUser> userManager,
            GeminiService geminiService,
            ILogger<FoodAnalysisController> logger,
            IConfiguration configuration)
        {
            _context = context;
            _mediaUrlService = mediaUrlService;
            _userManager = userManager;
            _geminiService = geminiService;
            _logger = logger;
            // Initialize Vietnam timezone (UTC+7)
            _vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById(configuration["TimeZone:Id"] ?? "SE Asia Standard Time");
        }

        /// <summary>
        /// Phân tích ảnh món ăn bằng AI để lấy thông tin dinh dưỡng
        /// </summary>
        [HttpPost("analyze")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> AnalyzeFood([FromForm] FoodAnalysisFormDto model)
        {
            var tempFilePath = string.Empty;

            try
            {
                // Step 1: Validate Input
                if (string.IsNullOrEmpty(model.UserId))
                {
                    return BadRequest("User ID is required");
                }

                if (model.Image == null || model.Image.Length == 0)
                {
                    return BadRequest("Invalid image file.");
                }

                // Kiểm tra content type
                if (!model.Image.ContentType.StartsWith("image/"))
                {
                    return BadRequest("Invalid image file.");
                }

                // Step 2: Save Physical File to Hotel_Web wwwroot
                var baseDirectory = Directory.GetCurrentDirectory();
                // Navigate from Hotel_API to Hotel_Web parent, then to Hotel_Web wwwroot
                var uploadsFolder = Path.Combine(baseDirectory, "..", "..", "Hotel_Web", "wwwroot", "uploads");
                uploadsFolder = Path.GetFullPath(uploadsFolder); // Normalize path

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = Guid.NewGuid() + Path.GetExtension(model.Image.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Image.CopyToAsync(fileStream);
                }

                // Step 3: Build Public URL
                var imageUrl = $"https://localhost:7043/uploads/{fileName}";

                // Step 4: Call Python API
                JsonElement prediction;
                bool isPythonSuccess = false;

                try
                {
                    prediction = await CallPythonApiAsync(filePath, model.Image.FileName);

                    string pythonMainDish = prediction.GetProperty("predicted_label").GetString() ?? "Unknown";
                    double pythonConfidence = prediction.GetProperty("confidence").GetDouble();

                    // Check if Python failed to recognize (confidence = 0 or "Không xác định")
                    if (pythonConfidence == 0 || pythonMainDish == "Không xác định" || string.IsNullOrEmpty(pythonMainDish))
                    {
                        _logger.LogWarning($"Python API failed to recognize food (confidence: {pythonConfidence}). Using Gemini as fallback.");
                        isPythonSuccess = false;
                    }
                    else
                    {
                        isPythonSuccess = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Python API error: {ex.Message}. Using Gemini as fallback.");
                    isPythonSuccess = false;
                    prediction = default;
                }

                // Step 4.5: Fallback to Gemini if Python failed
                double calories, fat, carbs, protein, confidence;
                string mainDish;

                if (isPythonSuccess)
                {
                    // Use Python results
                    mainDish = prediction.GetProperty("predicted_label").GetString() ?? "Unknown";
                    confidence = prediction.GetProperty("confidence").GetDouble();

                    var nutritionElement = prediction.GetProperty("nutrition");
                    calories = nutritionElement.GetProperty("calories").GetDouble();
                    fat = nutritionElement.GetProperty("fat").GetDouble();
                    carbs = nutritionElement.GetProperty("carbs").GetDouble();
                    protein = nutritionElement.GetProperty("protein").GetDouble();

                    _logger.LogInformation($"Using Python analysis: {mainDish}");
                }
                else
                {
                    // Use Gemini as fallback
                    _logger.LogInformation("Switching to Gemini API for food analysis");

                    var geminiResult = await _geminiService.AnalyzeFoodImageAsync(filePath);

                    mainDish = geminiResult.FoodName ?? "Không xác định";
                    confidence = geminiResult.Confidence;
                    calories = geminiResult.Calories;
                    fat = geminiResult.Fat;
                    carbs = geminiResult.Carbs;
                    protein = geminiResult.Protein;

                    _logger.LogInformation($"Using Gemini analysis: {mainDish} (confidence: {confidence})");
                }

                var mealTypeValue = string.IsNullOrEmpty(model.MealType) ? "lunch" : model.MealType.ToLower();

                // Step 5: Get User Health Plan (optional)
                var healthPlan = await _context.HealthPlans
                    .Where(p => p.UserId == model.UserId)
                    .OrderByDescending(p => p.NgayTao)
                    .FirstOrDefaultAsync();

                if (healthPlan == null)
                {
                    return BadRequest("Không tìm thấy phác đồ của người dùng.");
                }

                // Generate advice based on health plan
                var advice = GenerateAdvice(mainDish, calories, healthPlan);

                // Evaluate suitability with health plan
                int suitabilityScore = EvaluateSuitability(calories, healthPlan);
                string suggestions = GenerateSuggestions(mainDish, suitabilityScore, healthPlan);

                // Step 6: Save to Database
                var history = new PredictionHistory
                {
                    UserId = model.UserId,
                    ImagePath = imageUrl,
                    FoodName = mainDish,
                    Confidence = confidence,
                    Calories = calories,
                    Protein = protein,
                    Fat = fat,
                    Carbs = carbs,
                    MealType = mealTypeValue,
                    Reason = model.MealType,
                    Suitable = suitabilityScore.ToString(),
                    Suggestions = suggestions,
                    Advice = advice,
                    CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _vietnamTimeZone)
                };

                _context.PredictionHistories.Add(history);
                await _context.SaveChangesAsync();

                // Parse details from prediction and save as PredictionDetail records
                var detailsList = new List<object>();
                var predictionDetails = new List<PredictionDetail>();

                JsonElement detailsElement;
                if (prediction.TryGetProperty("details", out detailsElement))
                {
                    foreach (var detail in detailsElement.EnumerateArray())
                    {
                        var label = detail.GetProperty("label").GetString();
                        var weight = detail.GetProperty("weight").GetDouble();
                        var detailConfidence = detail.GetProperty("confidence").GetDouble();
                        var cal = detail.GetProperty("cal").GetDouble();
                        var detailFat = detail.GetProperty("fat").GetDouble();
                        var detailCarbs = detail.GetProperty("carbs").GetDouble();
                        var detailProtein = detail.GetProperty("protein").GetDouble();

                        detailsList.Add(new
                        {
                            label,
                            weight,
                            confidence = detailConfidence,
                            calories = cal,
                            protein = detailProtein,
                            fat = detailFat,
                            carbs = detailCarbs
                        });

                        // Create PredictionDetail record
                        var predictionDetail = new PredictionDetail
                        {
                            PredictionHistoryId = history.Id,
                            Label = label ?? "Unknown",
                            Weight = weight,
                            Calories = cal,
                            Protein = detailProtein,
                            Fat = detailFat,
                            Carbs = detailCarbs,
                            Confidence = detailConfidence
                        };
                        predictionDetails.Add(predictionDetail);
                    }
                }

                // Save all details
                if (predictionDetails.Any())
                {
                    _context.PredictionDetails.AddRange(predictionDetails);
                    await _context.SaveChangesAsync();
                }

                // Step 7: Return Response
                return Ok(new
                {
                    id = history.Id,
                    userId = model.UserId,
                    imagePath = imageUrl,
                    foodName = mainDish,
                    confidence,
                    calories,
                    protein,
                    fat,
                    carbs,
                    mealType = mealTypeValue,
                    advice,
                    createdAt = history.CreatedAt,
                    details = detailsList
                });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    title = "An error occurred while processing your request.",
                    status = 500,
                    detail = $"Python API error: {ex.Message}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    title = "An error occurred while processing your request.",
                    status = 500,
                    detail = ex.Message
                });
            }
            finally
            {
                // Step 8: Cleanup (if temp file was created)
                if (!string.IsNullOrEmpty(tempFilePath) && System.IO.File.Exists(tempFilePath))
                {
                    try
                    {
                        System.IO.File.Delete(tempFilePath);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Gọi Python API để phân tích ảnh
        /// </summary>
        private async Task<JsonElement> CallPythonApiAsync(string imagePath, string fileName)
        {
            using var httpClient = new HttpClient();
            using var form = new MultipartFormDataContent();

            var fileContent = new ByteArrayContent(await System.IO.File.ReadAllBytesAsync(imagePath));
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            form.Add(fileContent, "file", fileName);

            try
            {
                var response = await httpClient.PostAsync("http://127.0.0.1:5000/predict", form);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Python API error: {response.StatusCode} - {responseContent}");
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent, options);

                return result;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Python API connection error: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo lời khuyên dựa trên health plan của user
        /// </summary>
        private string GenerateAdvice(string foodName, double calories, HealthPlan? healthPlan)
        {
            if (healthPlan == null)
            {
                return $"Bữa ăn {foodName} này cung cấp khoảng {calories:F0} kcal. Hãy chắc chắn nó phù hợp với mục tiêu của bạn.";
            }

            // Parse calorie goal from DinhDuong (format: "2180 kcal; 66g P; 72g F; 311g C")
            var calorieGoal = 2000; // Default
            if (!string.IsNullOrEmpty(healthPlan.DinhDuong))
            {
                var parts = healthPlan.DinhDuong.Split(';');
                if (parts.Length > 0 && int.TryParse(parts[0].Replace("kcal", "").Trim(), out int parsedCalories))
                {
                    calorieGoal = parsedCalories;
                }
            }

            var remainingCalories = calorieGoal - calories;
            var isWithinLimit = remainingCalories >= 0;

            if (isWithinLimit)
            {
                return $"✓ Bữa ăn {foodName} này phù hợp với phác đồ của bạn. Calories: {calories:F0}/{calorieGoal}. " +
                       $"Còn lại: {remainingCalories:F0} kcal. {(healthPlan.KhuyenNghiMonAn != null ? "Nên ăn những thức ăn được khuyến khích." : "")}";
            }
            else
            {
                return $"⚠ Bữa ăn {foodName} này vượt quá mục tiêu calo của bạn. Calories: {calories:F0}/{calorieGoal}. " +
                       $"Bạn sẽ vượt {Math.Abs(remainingCalories):F0} kcal. Xem xét ăn bữa ăn nhẹ hơn.";
            }
        }

        /// <summary>
        /// Đánh giá mức độ phù hợp với phác đồ (0-100%)
        /// </summary>
        private int EvaluateSuitability(double calories, HealthPlan healthPlan)
        {
            if (healthPlan == null) return 50; // Neutral

            var calorieGoal = 2000;
            if (!string.IsNullOrEmpty(healthPlan.DinhDuong))
            {
                var parts = healthPlan.DinhDuong.Split(';');
                if (parts.Length > 0 && int.TryParse(parts[0].Replace("kcal", "").Trim(), out int parsedCalories))
                {
                    calorieGoal = parsedCalories;
                }
            }

            var percentage = (int)((calories * 100) / calorieGoal);
            return Math.Min(100, percentage); // Max 100%
        }

        /// <summary>
        /// Tạo gợi ý cải thiện
        /// </summary>
        private string GenerateSuggestions(string foodName, int suitabilityScore, HealthPlan healthPlan)
        {
            if (suitabilityScore > 90)
            {
                return "Bữa ăn này rất phù hợp! Hãy tiếp tục duy trì chế độ ăn uống lành mạnh.";
            }
            else if (suitabilityScore > 70)
            {
                return "Bữa ăn này tương đối phù hợp. Bạn có thể tăng thêm rau xanh để cân bằng hơn.";
            }
            else if (suitabilityScore > 50)
            {
                return "Bữa ăn này có chứa nhiều calo. Hãy chia nhỏ phần ăn hoặc luyện tập thêm.";
            }
            else
            {
                return "Bữa ăn này không phù hợp với phác đồ hiện tại. Xem xét chọn các thực phẩm thay thế lành mạnh hơn.";
            }
        }

        /// <summary>
        /// Lấy lịch sử phân tích của user (nếu có PredictionHistory table)
        /// </summary>
        [HttpGet("history/user/{userId}")]
        [ProducesResponseType(typeof(List<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<object>>> GetHistory(string userId)
        {
            try
            {
                var history = await _context.PredictionHistories
                    .Where(ph => ph.UserId == userId)
                    .OrderByDescending(ph => ph.CreatedAt)
                    .Select(ph => new
                    {
                        ph.Id,
                        ph.ImagePath,
                        ph.FoodName,
                        ph.Confidence,
                        ph.Calories,
                        ph.Protein,
                        ph.Fat,
                        ph.Carbs,
                        ph.MealType,
                        ph.CreatedAt,
                        ph.Advice
                    })
                    .ToListAsync();

                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Có lỗi xảy ra khi lấy lịch sử",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Xóa một bản ghi phân tích theo ID
        /// </summary>
        [HttpDelete("prediction/{idStr}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> DeleteHistory(string idStr)
        {
            try
            {
                // Parse string to int
                if (!int.TryParse(idStr, out int id))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "ID không hợp lệ",
                        errors = new[] { $"ID phải là số nguyên, nhưng nhận được: {idStr}" }
                    });
                }

                var history = await _context.PredictionHistories.FindAsync(id);
                if (history == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy bản ghi phân tích",
                        errors = new[] { $"PredictionHistory với ID {id} không tồn tại" }
                    });
                }

                // Xóa các PredictionDetail liên quan
                var details = await _context.PredictionDetails
                    .Where(pd => pd.PredictionHistoryId == id)
                    .ToListAsync();

                if (details.Any())
                {
                    _context.PredictionDetails.RemoveRange(details);
                }

                // Xóa PredictionHistory
                _context.PredictionHistories.Remove(history);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Xóa bản ghi phân tích thành công",
                    data = new
                    {
                        id = history.Id,
                        foodName = history.FoodName,
                        deletedAt = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi xóa",
                    errors = new[] { ex.Message, ex.StackTrace ?? "" }
                });
            }
        }

        /// <summary>
        /// Test endpoint - Kiểm tra xem URL có thể access được không
        /// </summary>
        [HttpGet("test-url/{filename}")]
        [AllowAnonymous]
        public ActionResult<object> TestUrl(string filename)
        {
            try
            {
                var mediaPath = $"/uploads/{filename}";
                var fullUrl = _mediaUrlService.GetFullMediaUrl(mediaPath);

                // Cố gắng tải file từ Hotel_Web
                var hotelWebUploadsPath = Path.Combine(
                    Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.FullName ?? "",
                    "Hotel_Web", "wwwroot", "uploads", filename);

                var fileExists = System.IO.File.Exists(hotelWebUploadsPath);

                return Ok(new
                {
                    filename = filename,
                    mediaPath = mediaPath,
                    fullUrl = fullUrl,
                    hotelWebPath = hotelWebUploadsPath,
                    fileExists = fileExists,
                    message = fileExists ? "File exists!" : "File not found"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Test failed",
                    message = ex.Message
                });
            }
        }
    }
}
