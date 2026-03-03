using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeTrackerWeb.Data;
using TimeTrackerWeb.Domain;

namespace TimeTrackerWeb.Controllers;

public class DashboardController : Controller
{
    private readonly AppDbContext _db;
    public DashboardController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(int? projectId = null, int? taskId = null)
    {
        var projects = await _db.Projects
            .Include(p => p.Tasks)
            .OrderByDescending(p => p.Id)
            .ToListAsync();

        Project? selectedProject = null;
        TaskItem? selectedTask = null;

        if (projects.Count > 0)
        {
            selectedProject = projectId != null
                ? projects.FirstOrDefault(p => p.Id == projectId)
                : projects.First();

            if (selectedProject != null && selectedProject.Tasks.Count > 0)
            {
                selectedTask = taskId != null
                    ? selectedProject.Tasks.FirstOrDefault(t => t.Id == taskId)
                    : selectedProject.Tasks.OrderByDescending(t => t.Id).First();
            }
        }

        var active = await _db.TimeEntries
            .Include(e => e.TaskItem)
            .ThenInclude(t => t.Project)
            .FirstOrDefaultAsync(e => e.EndTimeUtc == null);

        var recent = selectedTask == null
            ? new List<TimeEntry>()
            : await _db.TimeEntries
                .Where(e => e.TaskItemId == selectedTask.Id)
                .OrderByDescending(e => e.Id)
                .Take(30)
                .ToListAsync();

        var fromUtc = DateTime.Today.ToUniversalTime();
        var today = await _db.TimeEntries
            .Include(e => e.TaskItem)
            .Where(e => e.EndTimeUtc != null && e.StartTimeUtc >= fromUtc)
            .ToListAsync();

        var report = today
            .GroupBy(e => e.TaskItem!.Title)
            .Select(g => new ReportRow(g.Key, g.Sum(x => x.DurationSeconds)))
            .OrderByDescending(x => x.Seconds)
            .ToList();

        return View(new DashboardVm(projects, selectedProject, selectedTask, active, recent, report));
    }

    [HttpPost]
    public async Task<IActionResult> Start(int taskId)
    {
        var already = await _db.TimeEntries.FirstOrDefaultAsync(e => e.EndTimeUtc == null);
        if (already == null)
        {
            _db.TimeEntries.Add(new TimeEntry
            {
                TaskItemId = taskId,
                StartTimeUtc = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        // Повертаємось на Dashboard з вибраною задачею
        var task = await _db.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == taskId);
        return RedirectToAction(nameof(Index), new { projectId = task?.ProjectId, taskId = taskId });
    }

    [HttpPost]
    public async Task<IActionResult> Stop(int? projectId = null, int? taskId = null)
    {
        var active = await _db.TimeEntries.FirstOrDefaultAsync(e => e.EndTimeUtc == null);
        if (active != null)
        {
            active.EndTimeUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            taskId ??= active.TaskItemId;
        }

        if (projectId == null && taskId != null)
        {
            var task = await _db.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == taskId);
            projectId = task?.ProjectId;
        }

        return RedirectToAction(nameof(Index), new { projectId = projectId, taskId = taskId });
    }

    public record ReportRow(string TaskTitle, long Seconds);

    public record DashboardVm(
        List<Project> Projects,
        Project? SelectedProject,
        TaskItem? SelectedTask,
        TimeEntry? ActiveEntry,
        List<TimeEntry> RecentEntries,
        List<ReportRow> TodayReport
    );
}