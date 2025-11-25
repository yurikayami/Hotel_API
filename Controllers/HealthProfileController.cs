using Hotel_API.Data;
using Hotel_API.Models;
using Hotel_API.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hotel_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class HealthProfileController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HealthProfileController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Lấy hồ sơ sức khỏe của user
        /// </summary>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<object>>> GetHealthProfile()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Bạn cần đăng nhập"
                    });
                }

                var profile = _context.HealthProfiles.FirstOrDefault(h => h.UserId == userId);

                if (profile == null)
                {
                    profile = new HealthProfile
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.HealthProfiles.Add(profile);
                    await _context.SaveChangesAsync();
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Lấy hồ sơ sức khỏe thành công",
                    Data = profile
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Không thể lấy hồ sơ sức khỏe",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Cập nhật thông tin cá nhân
        /// </summary>
        [HttpPost("personal-info")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<object>>> UpdatePersonalInfo([FromBody] dynamic request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Bạn cần đăng nhập"
                    });
                }

                var profile = _context.HealthProfiles.FirstOrDefault(h => h.UserId == userId);

                if (profile == null)
                {
                    profile = new HealthProfile
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.HealthProfiles.Add(profile);
                }

                profile.FullName = request.fullName;
                profile.Age = request.age;
                profile.Gender = request.gender;
                profile.DateOfBirth = request.dateOfBirth;
                profile.BloodType = request.bloodType;
                profile.Weight = request.weight;
                profile.Height = request.height;
                profile.ActivityLevel = request.activityLevel;
                profile.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Cập nhật thông tin cá nhân thành công",
                    Data = profile
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi cập nhật",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Cập nhật thông tin bệnh mãn tính
        /// </summary>
        [HttpPost("chronic-conditions")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<object>>> UpdateChronicConditions([FromBody] dynamic request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Bạn cần đăng nhập"
                    });
                }

                var profile = _context.HealthProfiles.FirstOrDefault(h => h.UserId == userId);

                if (profile == null)
                {
                    profile = new HealthProfile
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.HealthProfiles.Add(profile);
                }

                profile.HasDiabetes = request.hasDiabetes ?? false;
                profile.HasHypertension = request.hasHypertension ?? false;
                profile.HasAsthma = request.hasAsthma ?? false;
                profile.HasHeartDisease = request.hasHeartDisease ?? false;
                profile.OtherDiseases = request.otherDiseases;
                profile.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Cập nhật thông tin bệnh mãn tính thành công",
                    Data = profile
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi cập nhật",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Kiểm tra độ hoàn thiện hồ sơ
        /// </summary>
        [HttpGet("completion")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public ActionResult<ApiResponse<object>> CheckCompletion()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var profile = _context.HealthProfiles.FirstOrDefault(h => h.UserId == userId);
                if (profile == null)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Data = new
                        {
                            completionPercentage = 0,
                            missingFields = new[] { "FullName", "Age", "Gender", "BloodType", "Height", "Weight" },
                            recommendations = new[] { "Vui lòng điền đầy đủ thông tin cá nhân" }
                        }
                    });
                }

                int completedFields = 0;
                int totalFields = 10;
                var missingFields = new List<string>();

                if (!string.IsNullOrEmpty(profile.FullName)) completedFields++; else missingFields.Add("FullName");
                if (profile.Age.HasValue) completedFields++; else missingFields.Add("Age");
                if (!string.IsNullOrEmpty(profile.Gender)) completedFields++; else missingFields.Add("Gender");
                if (profile.DateOfBirth.HasValue) completedFields++; else missingFields.Add("DateOfBirth");
                if (!string.IsNullOrEmpty(profile.BloodType)) completedFields++; else missingFields.Add("BloodType");
                if (profile.Height.HasValue) completedFields++; else missingFields.Add("Height");
                if (profile.Weight.HasValue) completedFields++; else missingFields.Add("Weight");
                if (!string.IsNullOrEmpty(profile.EmergencyContactName)) completedFields++; else missingFields.Add("EmergencyContactName");
                if (!string.IsNullOrEmpty(profile.EmergencyContactPhone)) completedFields++; else missingFields.Add("EmergencyContactPhone");
                if (!string.IsNullOrEmpty(profile.InsuranceNumber)) completedFields++; else missingFields.Add("InsuranceNumber");

                int completionPercentage = (int)((double)completedFields / totalFields * 100);

                var recommendations = new List<string>();
                if (missingFields.Contains("BloodType")) recommendations.Add("Bổ sung nhóm máu để phòng cấp cứu");
                if (missingFields.Contains("EmergencyContactPhone")) recommendations.Add("Thêm số điện thoại người thân");
                if (missingFields.Contains("InsuranceNumber")) recommendations.Add("Cập nhật thông tin bảo hiểm");

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new
                    {
                        completionPercentage,
                        missingFields = missingFields.ToArray(),
                        recommendations = recommendations.ToArray()
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}
