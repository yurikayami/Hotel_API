using System;
using System.Collections.Generic;

namespace Hotel_API.Models.ViewModels
{
    /// <summary>
    /// DTO cho xem thông tin hồ sơ người dùng (View Only)
    /// Combines data from ApplicationUser and HealthProfile with clean English naming
    /// </summary>
    public class UserProfileDto
    {
        // Identity / User Info (from ApplicationUser)
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty; // From gioi_tinh
        public int? Age { get; set; } // From tuoi

        // Physical Measurements (from HealthProfile)
        public double? Height { get; set; } // cm
        public double? Weight { get; set; } // kg
        public DateTime? DateOfBirth { get; set; }
        public string BloodType { get; set; } = string.Empty;

        // Emergency Contact Info (from HealthProfile)
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactPhone { get; set; } = string.Empty;

        // Insurance Info (from HealthProfile)
        public string InsuranceNumber { get; set; } = string.Empty;
        public string InsuranceProvider { get; set; } = string.Empty;

        // Medical Conditions (Boolean flags from HealthProfile)
        public bool HasDiabetes { get; set; }
        public bool HasHypertension { get; set; }
        public bool HasAsthma { get; set; }
        public bool HasHeartDisease { get; set; }
        public bool HasLatexAllergy { get; set; }

        // Medical Text Info (from HealthProfile)
        public string DrugAllergies { get; set; } = string.Empty;
        public string FoodAllergies { get; set; } = string.Empty;
        public string OtherDiseases { get; set; } = string.Empty;
        public string ActivityLevel { get; set; } = string.Empty;

        // Current Medications (parsed from CurrentMedicationsJson)
        public List<string> CurrentMedications { get; set; } = new List<string>();

        // Emergency Notes
        public string EmergencyNotes { get; set; } = string.Empty;

        // Metadata
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
