using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace Hotel_API.Services
{
    /// <summary>
    /// Service để gọi Gemini API cho việc nhận diện thực phẩm
    /// </summary>
    public class GeminiService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeminiService> _logger;
        private const string BaseUrl = "https://generativelanguage.googleapis.com/upload/files?key=";
        private const string ProcessUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=";

        public GeminiService(HttpClient httpClient, ILogger<GeminiService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini:ApiKey not configured");
        }

        /// <summary>
        /// Phân tích ảnh thực phẩm bằng Gemini Vision API
        /// </summary>
        /// <param name="imagePath">Đường dẫn file ảnh trên server</param>
        /// <returns>Kết quả phân tích chứa tên thực phẩm, calories, protein, fat, carbs</returns>
        public async Task<GeminiAnalysisResult> AnalyzeFoodImageAsync(string imagePath)
        {
            try
            {
                _logger.LogInformation($"Analyzing food image with Gemini: {imagePath}");

                // Read image file
                if (!File.Exists(imagePath))
                {
                    throw new FileNotFoundException($"Image file not found: {imagePath}");
                }

                var imageBytes = await File.ReadAllBytesAsync(imagePath);
                var base64Image = Convert.ToBase64String(imageBytes);

                // Determine media type from file extension
                var extension = Path.GetExtension(imagePath).ToLower();
                var mediaType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".webp" => "image/webp",
                    _ => "image/jpeg"
                };

                // Create request payload
                var requestPayload = new
                {
                    contents = new object[]
                    {
                        new
                        {
                            parts = new object[]
                            {
                                new
                                {
                                    text = @"Analyze this food image and provide nutritional information in JSON format. 
                                    
                                    Respond ONLY with valid JSON in this exact format, no other text:
                                    {
                                        ""foodName"": ""name of the food dish"",
                                        ""description"": ""brief description"",
                                        ""confidence"": 0.85,
                                        ""calories"": 250,
                                        ""protein"": 15,
                                        ""fat"": 8,
                                        ""carbs"": 30,
                                        ""servingSize"": ""100g""
                                    }
                                    
                                    If you cannot identify the food, use:
                                    {
                                        ""foodName"": ""Không xác định"",
                                        ""description"": ""Không thể nhận diện thực phẩm"",
                                        ""confidence"": 0,
                                        ""calories"": 0,
                                        ""protein"": 0,
                                        ""fat"": 0,
                                        ""carbs"": 0,
                                        ""servingSize"": ""unknown""
                                    }
                                    
                                    Be as accurate as possible with nutritional values for Vietnamese dishes."
                                },
                                new
                                {
                                    inlineData = new
                                    {
                                        mimeType = mediaType,
                                        data = base64Image
                                    }
                                }
                            }
                        }
                    }
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestPayload),
                    Encoding.UTF8,
                    "application/json");

                // Call Gemini API
                var response = await _httpClient.PostAsync(
                    $"{ProcessUrl}{_apiKey}",
                    jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Gemini API error: {response.StatusCode} - {errorContent}");
                    throw new Exception($"Gemini API error: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = ParseGeminiResponse(responseContent);

                _logger.LogInformation($"Gemini analysis completed: {result.FoodName}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing food image with Gemini");
                throw;
            }
        }

        /// <summary>
        /// Parse Gemini API response
        /// </summary>
        private GeminiAnalysisResult ParseGeminiResponse(string responseContent)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                using var jsonDoc = JsonDocument.Parse(responseContent);
                var root = jsonDoc.RootElement;

                // Navigate to the text content
                var candidates = root.GetProperty("candidates");
                if (candidates.GetArrayLength() == 0)
                {
                    throw new Exception("No candidates in Gemini response");
                }

                var firstCandidate = candidates[0];
                var content = firstCandidate.GetProperty("content");
                var parts = content.GetProperty("parts");

                if (parts.GetArrayLength() == 0)
                {
                    throw new Exception("No parts in Gemini response");
                }

                var textPart = parts[0];
                var text = textPart.GetProperty("text").GetString();

                // Extract JSON from text (handle cases where Gemini might add extra text)
                var jsonMatch = System.Text.RegularExpressions.Regex.Match(
                    text ?? "",
                    @"\{[\s\S]*\}",
                    System.Text.RegularExpressions.RegexOptions.Multiline);

                if (!jsonMatch.Success)
                {
                    _logger.LogWarning($"Could not extract JSON from Gemini response: {text}");
                    return new GeminiAnalysisResult
                    {
                        FoodName = "Không xác định",
                        Description = "Không thể nhận diện",
                        Confidence = 0,
                        Calories = 0,
                        Protein = 0,
                        Fat = 0,
                        Carbs = 0
                    };
                }

                var jsonStr = jsonMatch.Value;
                var analysisData = JsonSerializer.Deserialize<GeminiAnalysisResult>(jsonStr, options);

                return analysisData ?? throw new Exception("Failed to deserialize Gemini response");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Gemini response");
                throw;
            }
        }
    }

    /// <summary>
    /// Result from Gemini food analysis
    /// </summary>
    public class GeminiAnalysisResult
    {
        public string FoodName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public double Calories { get; set; }
        public double Protein { get; set; }
        public double Fat { get; set; }
        public double Carbs { get; set; }
        public string? ServingSize { get; set; }
    }
}
