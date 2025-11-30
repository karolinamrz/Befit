using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BeFit.Models
{
    public class AppUser : IdentityUser
    {
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Height (cm)")]
        public int? Height { get; set; }

        [Display(Name = "Weight (kg)")]
        public decimal? Weight { get; set; }
    }
}