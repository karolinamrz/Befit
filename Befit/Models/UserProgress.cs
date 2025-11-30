using System.ComponentModel.DataAnnotations;

namespace BeFit.Models
{
    public class UserProgress
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime ProgressDate { get; set; } = DateTime.Today;

        [Display(Name = "Weight (kg)")]
        [Range(0, 300)]
        public decimal? Weight { get; set; }

        [Display(Name = "Height (cm)")]
        [Range(0, 250)]
        public int? Height { get; set; }

        [Display(Name = "Chest (cm)")]
        [Range(0, 150)]
        public decimal? ChestMeasurement { get; set; }

        [Display(Name = "Waist (cm)")]
        [Range(0, 150)]
        public decimal? WaistMeasurement { get; set; }

        [Display(Name = "Arms (cm)")]
        [Range(0, 100)]
        public decimal? ArmsMeasurement { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        public string AppUserId { get; set; } = string.Empty;
        public virtual AppUser AppUser { get; set; } = null!;
    }
}