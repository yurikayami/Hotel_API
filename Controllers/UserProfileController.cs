using Hotel_API.Data;
using Hotel_API.Models;
using Hotel_API.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using System.Globalization;

namespace Hotel_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserProfileController> _logger;

        public UserProfileController(AppDbContext context, ILogger<UserProfileController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/userprofile/profile
        /// Lấy thông tin hồ sơ người dùng (bao gồm thông tin từ ApplicationUser và HealthProfile)
        /// </summary>
        /// <returns>UserProfileDto với đầy đủ thông tin cá nhân và sức khỏe</returns>
        [HttpGet("profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            try
            {
                // Lấy UserId từ token
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UserId not found in token");
                    return Unauthorized(new { message = "User not identified" });
                }

                // Lấy ApplicationUser
                var applicationUser = await _context.Users.FindAsync(userId);
                if (applicationUser == null)
                {
                    _logger.LogWarning($"User with ID {userId} not found");
                    return NotFound(new { message = "User not found" });
                }

                // Lấy HealthProfile
                var healthProfile = _context.HealthProfiles.FirstOrDefault(hp => hp.UserId == userId);

                // Map dữ liệu sang UserProfileDto
                var userProfileDto = MapToUserProfileDto(applicationUser, healthProfile);

                _logger.LogInformation($"Successfully retrieved profile for user {userId}");
                return Ok(userProfileDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving profile: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while retrieving profile", error = ex.Message });
            }
        }

        /// <summary>
        /// PUT /api/userprofile/profile
        /// Cập nhật thông tin hồ sơ người dùng với logic sync giữa ApplicationUser và HealthProfile
        /// </summary>
        /// <param name="updateProfileDto">Dữ liệu cập nhật</param>
        /// <returns>UserProfileDto cập nhật</returns>
        [HttpPut("profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileDto updateProfileDto)
        {
            try
            {
                // Validate input
                if (updateProfileDto == null)
                {
                    return BadRequest(new { message = "Invalid request body" });
                }

                // Lấy UserId từ token
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UserId not found in token");
                    return Unauthorized(new { message = "User not identified" });
                }

                // Lấy ApplicationUser
                var applicationUser = await _context.Users.FindAsync(userId);
                if (applicationUser == null)
                {
                    _logger.LogWarning($"User with ID {userId} not found");
                    return NotFound(new { message = "User not found" });
                }

                // Lấy HealthProfile hoặc tạo mới nếu chưa tồn tại
                var healthProfile = _context.HealthProfiles.FirstOrDefault(hp => hp.UserId == userId);
                if (healthProfile == null)
                {
                    healthProfile = new HealthProfile
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.HealthProfiles.Add(healthProfile);
                }

                // Cập nhật ApplicationUser fields
                if (!string.IsNullOrEmpty(updateProfileDto.DisplayName))
                {
                    applicationUser.displayName = updateProfileDto.DisplayName;
                }

                if (!string.IsNullOrEmpty(updateProfileDto.Avatar))
                {
                    applicationUser.avatarUrl = updateProfileDto.Avatar;
                }

                // ✓ CRITICAL SYNC LOGIC: Gender
                if (!string.IsNullOrEmpty(updateProfileDto.Gender))
                {
                    // Update both ApplicationUser.gioi_tinh and HealthProfile.Gender
                    applicationUser.gioi_tinh = updateProfileDto.Gender;
                    healthProfile.Gender = updateProfileDto.Gender;
                }

                // ✓ CRITICAL SYNC LOGIC: DateOfBirth & Age
                if (updateProfileDto.DateOfBirth.HasValue)
                {
                    healthProfile.DateOfBirth = updateProfileDto.DateOfBirth.Value;

                    // Auto-calculate age from DateOfBirth
                    int calculatedAge = CalculateAge(updateProfileDto.DateOfBirth.Value);
                    applicationUser.tuoi = calculatedAge;
                    healthProfile.Age = calculatedAge;
                }
                else if (updateProfileDto.Age.HasValue)
                {
                    // If only Age is provided, update both
                    applicationUser.tuoi = updateProfileDto.Age.Value;
                    healthProfile.Age = updateProfileDto.Age.Value;
                }

                // Cập nhật HealthProfile fields
                if (updateProfileDto.Height.HasValue)
                {
                    healthProfile.Height = updateProfileDto.Height.Value;
                }

                if (updateProfileDto.Weight.HasValue)
                {
                    healthProfile.Weight = updateProfileDto.Weight.Value;
                }

                if (!string.IsNullOrEmpty(updateProfileDto.BloodType))
                {
                    healthProfile.BloodType = updateProfileDto.BloodType;
                }

                if (!string.IsNullOrEmpty(updateProfileDto.EmergencyContactName))
                {
                    healthProfile.EmergencyContactName = updateProfileDto.EmergencyContactName;
                }

                if (!string.IsNullOrEmpty(updateProfileDto.EmergencyContactPhone))
                {
                    healthProfile.EmergencyContactPhone = updateProfileDto.EmergencyContactPhone;
                }

                if (!string.IsNullOrEmpty(updateProfileDto.InsuranceNumber))
                {
                    healthProfile.InsuranceNumber = updateProfileDto.InsuranceNumber;
                }

                if (!string.IsNullOrEmpty(updateProfileDto.InsuranceProvider))
                {
                    healthProfile.InsuranceProvider = updateProfileDto.InsuranceProvider;
                }

                // Medical condition flags
                healthProfile.HasDiabetes = updateProfileDto.HasDiabetes;
                healthProfile.HasHypertension = updateProfileDto.HasHypertension;
                healthProfile.HasAsthma = updateProfileDto.HasAsthma;
                healthProfile.HasHeartDisease = updateProfileDto.HasHeartDisease;
                healthProfile.HasLatexAllergy = updateProfileDto.HasLatexAllergy;

                // Medical text fields
                if (!string.IsNullOrEmpty(updateProfileDto.DrugAllergies))
                {
                    healthProfile.DrugAllergies = updateProfileDto.DrugAllergies;
                }

                if (!string.IsNullOrEmpty(updateProfileDto.FoodAllergies))
                {
                    healthProfile.FoodAllergies = updateProfileDto.FoodAllergies;
                }

                if (!string.IsNullOrEmpty(updateProfileDto.OtherDiseases))
                {
                    healthProfile.OtherDiseases = updateProfileDto.OtherDiseases;
                }

                if (!string.IsNullOrEmpty(updateProfileDto.ActivityLevel))
                {
                    healthProfile.ActivityLevel = updateProfileDto.ActivityLevel;
                }

                if (!string.IsNullOrEmpty(updateProfileDto.CurrentMedicationsJson))
                {
                    healthProfile.CurrentMedicationsJson = updateProfileDto.CurrentMedicationsJson;
                }

                if (!string.IsNullOrEmpty(updateProfileDto.EmergencyNotes))
                {
                    healthProfile.EmergencyNotes = updateProfileDto.EmergencyNotes;
                }

                // Update timestamp
                healthProfile.UpdatedAt = DateTime.UtcNow;

                // Save all changes in a single transaction
                _context.Users.Update(applicationUser);
                _context.HealthProfiles.Update(healthProfile);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully updated profile for user {userId}");

                // Return updated profile
                var updatedProfileDto = MapToUserProfileDto(applicationUser, healthProfile);
                return Ok(updatedProfileDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating profile: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while updating profile", error = ex.Message });
            }
        }

        /// <summary>
        /// Map ApplicationUser và HealthProfile sang UserProfileDto
        /// </summary>
        private UserProfileDto MapToUserProfileDto(ApplicationUser applicationUser, HealthProfile? healthProfile)
        {
            var dto = new UserProfileDto
            {
                UserId = applicationUser.Id,
                DisplayName = applicationUser.displayName ?? string.Empty,
                Avatar = applicationUser.avatarUrl ?? string.Empty,
                Email = applicationUser.Email ?? string.Empty,
                PhoneNumber = applicationUser.PhoneNumber ?? string.Empty,
                Gender = applicationUser.gioi_tinh ?? string.Empty,
                Age = applicationUser.tuoi
            };

            if (healthProfile != null)
            {
                dto.Height = healthProfile.Height;
                dto.Weight = healthProfile.Weight;
                dto.DateOfBirth = healthProfile.DateOfBirth;
                dto.BloodType = healthProfile.BloodType ?? string.Empty;
                dto.EmergencyContactName = healthProfile.EmergencyContactName ?? string.Empty;
                dto.EmergencyContactPhone = healthProfile.EmergencyContactPhone ?? string.Empty;
                dto.InsuranceNumber = healthProfile.InsuranceNumber ?? string.Empty;
                dto.InsuranceProvider = healthProfile.InsuranceProvider ?? string.Empty;
                dto.HasDiabetes = healthProfile.HasDiabetes;
                dto.HasHypertension = healthProfile.HasHypertension;
                dto.HasAsthma = healthProfile.HasAsthma;
                dto.HasHeartDisease = healthProfile.HasHeartDisease;
                dto.HasLatexAllergy = healthProfile.HasLatexAllergy;
                dto.DrugAllergies = healthProfile.DrugAllergies ?? string.Empty;
                dto.FoodAllergies = healthProfile.FoodAllergies ?? string.Empty;
                dto.OtherDiseases = healthProfile.OtherDiseases ?? string.Empty;
                dto.ActivityLevel = healthProfile.ActivityLevel ?? string.Empty;
                dto.EmergencyNotes = healthProfile.EmergencyNotes ?? string.Empty;
                dto.CreatedAt = healthProfile.CreatedAt;
                dto.UpdatedAt = healthProfile.UpdatedAt;

                // Parse CurrentMedicationsJson if available
                if (!string.IsNullOrEmpty(healthProfile.CurrentMedicationsJson))
                {
                    try
                    {
                        var medications = JsonSerializer.Deserialize<List<string>>(healthProfile.CurrentMedicationsJson);
                        dto.CurrentMedications = medications ?? new List<string>();
                    }
                    catch
                    {
                        // If JSON parsing fails, treat as single string
                        dto.CurrentMedications = new List<string> { healthProfile.CurrentMedicationsJson };
                    }
                }
            }

            return dto;
        }

        /// <summary>
        /// Tính tuổi từ ngày sinh
        /// </summary>
        private int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;

            if (dateOfBirth.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }
}
