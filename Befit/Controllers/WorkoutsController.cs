using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using BeFit.Data;
using BeFit.Models;
using System.Security.Claims;

namespace BeFit.Controllers
{
    [Authorize]
    public class WorkoutsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public WorkoutsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var workouts = await _context.Workouts
                .Where(w => w.UserId == userId)
                .ToListAsync();

            return View(workouts);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            var workout = await _context.Workouts
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (workout == null)
                return NotFound();

            return View(workout);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Workout workout)
        {
            if (ModelState.IsValid)
            {
                workout.UserId = _userManager.GetUserId(User);
                _context.Add(workout);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(workout);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            var workout = await _context.Workouts
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (workout == null)
                return NotFound();

            return View(workout);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Workout workout)
        {
            if (id != workout.Id)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            var existingWorkout = await _context.Workouts
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

            if (existingWorkout == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    existingWorkout.StartDate = workout.StartDate;
                    existingWorkout.EndDate = workout.EndDate;
                    existingWorkout.Notes = workout.Notes;

                    _context.Update(existingWorkout);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkoutExists(workout.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(workout);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            var workout = await _context.Workouts
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (workout == null)
                return NotFound();

            return View(workout);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var workout = await _context.Workouts
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

            if (workout != null)
            {
                _context.Workouts.Remove(workout);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WorkoutExists(int id)
        {
            return _context.Workouts.Any(e => e.Id == id);
        }
    }
}