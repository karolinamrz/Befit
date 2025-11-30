using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using BeFit.Data;
using BeFit.Models;

namespace BeFit.Controllers
{
    [Authorize]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public StatisticsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var fourWeeksAgo = DateTime.Now.AddDays(-28);

            var userWorkouts = await _context.Workouts
                .Where(w => w.UserId == userId && w.StartDate >= fourWeeksAgo)
                .Select(w => w.Id)
                .ToListAsync();

            var performedExercises = await _context.PerformedExercises
                .Where(pe => userWorkouts.Contains(pe.WorkoutId))
                .Include(pe => pe.ExerciseType)
                .ToListAsync();

            var stats = performedExercises
                .GroupBy(pe => pe.ExerciseTypeId)
                .Select(g => new StatisticsViewModel
                {
                    ExerciseTypeName = g.First().ExerciseType?.Name ?? "Nieznane",
                    TimesPerformed = g.Count(),
                    TotalRepetitions = g.Sum(pe => pe.Sets * pe.Reps),
                    AverageWeight = g.Average(pe => pe.Weight),
                    MaxWeight = g.Max(pe => pe.Weight)
                })
                .ToList();

            return View(stats);
        }
    }

    public class StatisticsViewModel
    {
        public string ExerciseTypeName { get; set; }
        public int TimesPerformed { get; set; }
        public int TotalRepetitions { get; set; }
        public double AverageWeight { get; set; }
        public double MaxWeight { get; set; }
    }
}