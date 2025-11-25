using System;
using System.Collections.Generic;

namespace Hotel_API.Models.ViewModels
{
    /// <summary>
    /// DTO cho cập nhật thông tin hồ sơ người dùng (Edit Profile)
    /// Only includes fields that can be updated by the user
    /// </summary>
    public class UpdateProfileDto
    {
        // Identity / User Info (updatable from ApplicationUser)
        public string DisplayName { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty; // Will sync to gioi_tinh in ApplicationUser
        public int? Age { get; set; } // Will sync to tuoi in ApplicationUser

        // Physical Measurements (updatable in HealthProfile)
        public double? Height { get; set; } // cm
        public double? Weight { get; set; } // kg
        public DateTime? DateOfBirth { get; set; } // Will auto-calculate Age if provided
        public string BloodType { get; set; } = string.Empty;

        // Emergency Contact Info (updatable in HealthProfile)
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactPhone { get; set; } = string.Empty;

        // Insurance Info (updatable in HealthProfile)
        public string InsuranceNumber { get; set; } = string.Empty;
        public string InsuranceProvider { get; set; } = string.Empty;

        // Medical Conditions (Boolean flags updatable in HealthProfile)
        public bool HasDiabetes { get; set; }
        public bool HasHypertension { get; set; }
        public bool HasAsthma { get; set; }
        public bool HasHeartDisease { get; set; }
        public bool HasLatexAllergy { get; set; }

        // Medical Text Info (updatable in HealthProfile)
        public string DrugAllergies { get; set; } = string.Empty;
        public string FoodAllergies { get; set; } = string.Empty;
        public string OtherDiseases { get; set; } = string.Empty;
        public string ActivityLevel { get; set; } = string.Empty;

        // Current Medications (as JSON string or comma-separated)
        public string CurrentMedicationsJson { get; set; } = string.Empty;

        // Emergency Notes
        public string EmergencyNotes { get; set; } = string.Empty;
    }
}
