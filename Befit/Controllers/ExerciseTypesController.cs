using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using BeFit.Data;
using BeFit.Models;
using System.Data;

namespace BeFit.Controllers
{
    [Authorize]
    public class ExerciseTypesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ExerciseTypesController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var exerciseTypes = await _context.ExerciseTypes.ToListAsync();
            return View(exerciseTypes);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(ExerciseType exerciseType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(exerciseType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(exerciseType);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var exerciseType = await _context.ExerciseTypes.FindAsync(id);
            if (exerciseType == null)
                return NotFound();

            return View(exerciseType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, ExerciseType exerciseType)
        {
            if (id != exerciseType.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(exerciseType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExerciseTypeExists(exerciseType.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(exerciseType);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var exerciseType = await _context.ExerciseTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (exerciseType == null)
                return NotFound();

            return View(exerciseType);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var exerciseType = await _context.ExerciseTypes.FindAsync(id);
            if (exerciseType != null)
            {
                _context.ExerciseTypes.Remove(exerciseType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExerciseTypeExists(int id)
        {
            return _context.ExerciseTypes.Any(e => e.Id == id);
        }
    }
}