using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TimeTrackerWeb.Data;
using TimeTrackerWeb.Domain;

namespace TimeTrackerWeb.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly AppDbContext _context;

        public ProjectsController(AppDbContext context)
        {
            _context = context;
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (userId == null) return Challenge();

            var projects = await _context.Projects
                .Where(p => p.UserId == userId)
                .Include(p => p.Tasks)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(projects);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Challenge();

            var project = await _context.Projects
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.TimeEntries)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (project == null)
                return NotFound();

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name)
        {
            var userId = GetUserId();
            if (userId == null) return Challenge();

            if (string.IsNullOrWhiteSpace(name))
                return RedirectToAction(nameof(Index));

            var project = new Project
            {
                Name = name.Trim(),
                UserId = userId
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Challenge();

            var project = await _context.Projects
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.TimeEntries)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (project == null)
                return NotFound();

            foreach (var task in project.Tasks)
            {
                _context.TimeEntries.RemoveRange(task.TimeEntries);
            }

            _context.Tasks.RemoveRange(project.Tasks);
            _context.Projects.Remove(project);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}