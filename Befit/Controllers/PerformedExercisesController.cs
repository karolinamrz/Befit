using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using BeFit.Data;
using BeFit.Models;
using System.Linq;
using System.Threading.Tasks;

namespace BeFit.Controllers
{
    [Authorize]
    public class PerformedExercisesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public PerformedExercisesController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var performedExercises = await _context.PerformedExercises
                .Include(pe => pe.Workout)
                .Include(pe => pe.ExerciseType)
                .Where(pe => pe.Workout.UserId == userId)
                .ToListAsync();

            return View(performedExercises);
        }

        public async Task<IActionResult> Create()
        {
            await LoadViewData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PerformedExercise performedExercise)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var workout = await _context.Workouts
                    .FirstOrDefaultAsync(w => w.Id == performedExercise.WorkoutId && w.UserId == userId);

                if (workout == null)
                {
                    ModelState.AddModelError("", "Nieprawidłowy trening");
                    await LoadViewData();
                    return View(performedExercise);
                }

                _context.Add(performedExercise);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await LoadViewData();
            return View(performedExercise);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            var performedExercise = await _context.PerformedExercises
                .Include(pe => pe.Workout)
                .Include(pe => pe.ExerciseType)
                .FirstOrDefaultAsync(pe => pe.Id == id && pe.Workout.UserId == userId);

            if (performedExercise == null)
                return NotFound();

            return View(performedExercise);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var performedExercise = await _context.PerformedExercises
                .Include(pe => pe.Workout)
                .FirstOrDefaultAsync(pe => pe.Id == id && pe.Workout.UserId == userId);

            if (performedExercise != null)
            {
                _context.PerformedExercises.Remove(performedExercise);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadViewData()
        {
            var userId = _userManager.GetUserId(User);
            ViewData["WorkoutId"] = await _context.Workouts
                .Where(w => w.UserId == userId)
                .Select(w => new { w.Id, Description = $"Trening {w.StartDate:dd.MM.yyyy HH:mm}" })
                .ToListAsync();

            ViewData["ExerciseTypeId"] = await _context.ExerciseTypes
                .Select(et => new { et.Id, et.Name })
                .ToListAsync();
        }
    }
}