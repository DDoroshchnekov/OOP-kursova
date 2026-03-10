using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TimeTrackerWeb.Data;
using TimeTrackerWeb.Domain;

namespace TimeTrackerWeb.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Challenge();

            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.TimeEntries.OrderByDescending(te => te.StartTimeUtc))
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
                return NotFound();

            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int projectId, string title)
        {
            var userId = GetUserId();
            if (userId == null) return Challenge();

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

            if (project == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(title))
                return RedirectToAction("Details", "Projects", new { id = projectId });

            var task = new TaskItem
            {
                Title = title.Trim(),
                ProjectId = projectId,
                UserId = userId,
                IsActive = false
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Projects", new { id = projectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int taskId)
        {
            var userId = GetUserId();
            if (userId == null) return Challenge();

            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);

            if (task == null)
                return NotFound();

            var hasRunningEntry = await _context.TimeEntries.AnyAsync(te =>
                te.TaskItemId == taskId &&
                te.UserId == userId &&
                te.EndTimeUtc == null);

            if (!hasRunningEntry)
            {
                var entry = new TimeEntry
                {
                    TaskItemId = taskId,
                    UserId = userId,
                    StartTimeUtc = DateTime.UtcNow,
                    EndTimeUtc = null,
                    DurationSeconds = 0
                };

                _context.TimeEntries.Add(entry);
            }

            task.IsActive = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = taskId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Stop(int taskId)
        {
            var userId = GetUserId();
            if (userId == null) return Challenge();

            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);

            if (task == null)
                return NotFound();

            var entry = await _context.TimeEntries
                .Where(te => te.TaskItemId == taskId && te.UserId == userId && te.EndTimeUtc == null)
                .OrderByDescending(te => te.StartTimeUtc)
                .FirstOrDefaultAsync();

            if (entry != null)
            {
                entry.EndTimeUtc = DateTime.UtcNow;
                entry.DurationSeconds = (int)(entry.EndTimeUtc.Value - entry.StartTimeUtc).TotalSeconds;
            }

            task.IsActive = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = taskId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Challenge();

            var task = await _context.Tasks
                .Include(t => t.TimeEntries)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
                return NotFound();

            var projectId = task.ProjectId;

            _context.TimeEntries.RemoveRange(task.TimeEntries);
            _context.Tasks.Remove(task);

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Projects", new { id = projectId });
        }

        [HttpGet]
        public async Task<IActionResult> ReportToday()
        {
            var userId = GetUserId();
            if (userId == null) return Challenge();

            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var entries = await _context.TimeEntries
                .Include(te => te.TaskItem)
                    .ThenInclude(t => t!.Project)
                .Where(te => te.UserId == userId &&
                             te.StartTimeUtc >= today &&
                             te.StartTimeUtc < tomorrow)
                .OrderByDescending(te => te.StartTimeUtc)
                .ToListAsync();

            return View(entries);
        }
    }
}