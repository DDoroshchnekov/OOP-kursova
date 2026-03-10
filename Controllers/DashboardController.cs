using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TimeTrackerWeb.Data;

namespace TimeTrackerWeb.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
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

            var projectsCount = await _context.Projects
                .CountAsync(p => p.UserId == userId);

            var tasksCount = await _context.Tasks
                .CountAsync(t => t.UserId == userId);

            var activeTasksCount = await _context.Tasks
                .CountAsync(t => t.UserId == userId && t.IsActive);

            var totalSeconds = await _context.TimeEntries
                .Where(te => te.UserId == userId && te.EndTimeUtc != null)
                .SumAsync(te => (int?)te.DurationSeconds) ?? 0;

            var recentEntries = await _context.TimeEntries
                .Include(te => te.TaskItem)
                    .ThenInclude(t => t!.Project)
                .Where(te => te.UserId == userId)
                .OrderByDescending(te => te.StartTimeUtc)
                .Take(10)
                .ToListAsync();

            ViewBag.ProjectsCount = projectsCount;
            ViewBag.TasksCount = tasksCount;
            ViewBag.ActiveTasksCount = activeTasksCount;
            ViewBag.TotalSeconds = totalSeconds;

            return View(recentEntries);
        }
    }
}