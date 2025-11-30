using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace BeFit.Models
{
    public class Workout
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Data rozpoczęcia jest wymagana")]
        [Display(Name = "Data rozpoczęcia")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Data zakończenia jest wymagana")]
        [Display(Name = "Data zakończenia")]
        public DateTime EndDate { get; set; }

        [StringLength(1000, ErrorMessage = "Notatki mogą mieć maksymalnie 1000 znaków")]
        [Display(Name = "Notatki")]
        public string Notes { get; set; }

        public string UserId { get; set; }

        public ICollection<PerformedExercise> PerformedExercises { get; set; }
    }
}