using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_API.Models
{
    /// <summary>
    /// Model cho Hồ Sơ Sức Khỏe (Health Profile)
    /// </summary>
    [Table("HealthProfile")]
    public class HealthProfile
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? ApplicationUser { get; set; }

        [MaxLength(255)]
        public string? FullName { get; set; }

        public int? Age { get; set; }

        [MaxLength(50)]
        public string? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(10)]
        public string? BloodType { get; set; }

        [MaxLength(255)]
        public string? EmergencyContactName { get; set; }

        [MaxLength(20)]
        public string? EmergencyContactPhone { get; set; }

        // Chronic Conditions
        public bool HasDiabetes { get; set; }

        public bool HasHypertension { get; set; }

        public bool HasAsthma { get; set; }

        public bool HasHeartDisease { get; set; }

        [MaxLength(500)]
        public string? OtherDiseases { get; set; }

        // Allergies
        [MaxLength(500)]
        public string? DrugAllergies { get; set; }

        [MaxLength(500)]
        public string? FoodAllergies { get; set; }

        public bool HasLatexAllergy { get; set; }

        // Current Medications (stored as JSON)
        public string? CurrentMedicationsJson { get; set; }

        // Insurance
        [MaxLength(50)]
        public string? InsuranceNumber { get; set; }

        [MaxLength(255)]
        public string? InsuranceProvider { get; set; }

        [MaxLength(1000)]
        public string? EmergencyNotes { get; set; }

        // Physical Measurements
        public double? Weight { get; set; } // kg

        public double? Height { get; set; } // cm

        [MaxLength(50)]
        public string? ActivityLevel { get; set; } // Sedentary, Light, Moderate, Active, VeryActive

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
