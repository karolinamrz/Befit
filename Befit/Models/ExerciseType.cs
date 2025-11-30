using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace BeFit.Models
{
    public class ExerciseType
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa ćwiczenia jest wymagana")]
        [StringLength(100, ErrorMessage = "Nazwa może mieć maksymalnie 100 znaków")]
        [Display(Name = "Nazwa ćwiczenia")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Opis może mieć maksymalnie 500 znaków")]
        [Display(Name = "Opis")]
        public string Description { get; set; }

        [StringLength(50, ErrorMessage = "Grupa mięśniowa może mieć maksymalnie 50 znaków")]
        [Display(Name = "Grupa mięśniowa")]
        public string MuscleGroup { get; set; }

        public ICollection<PerformedExercise> PerformedExercises { get; set; }
    }
}