using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeFit.Models
{
    public class PerformedExercise
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Trening jest wymagany")]
        [Display(Name = "Trening")]
        public int WorkoutId { get; set; }

        [Required(ErrorMessage = "Typ ćwiczenia jest wymagany")]
        [Display(Name = "Typ ćwiczenia")]
        public int ExerciseTypeId { get; set; }

        [Required(ErrorMessage = "Liczba serii jest wymagana")]
        [Range(1, 20, ErrorMessage = "Liczba serii musi być między 1 a 20")]
        [Display(Name = "Liczba serii")]
        public int Sets { get; set; }

        [Required(ErrorMessage = "Liczba powtórzeń jest wymagana")]
        [Range(1, 100, ErrorMessage = "Liczba powtórzeń musi być między 1 a 100")]
        [Display(Name = "Liczba powtórzeń w serii")]
        public int Reps { get; set; }

        [Required(ErrorMessage = "Obciążenie jest wymagane")]
        [Range(0.1, 500, ErrorMessage = "Obciążenie musi być między 0.1 a 500 kg")]
        [Display(Name = "Obciążenie (kg)")]
        public double Weight { get; set; }

        [ForeignKey("WorkoutId")]
        public Workout Workout { get; set; }

        [ForeignKey("ExerciseTypeId")]
        public ExerciseType ExerciseType { get; set; }
    }
}