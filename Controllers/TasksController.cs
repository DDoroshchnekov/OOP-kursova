using Microsoft.AspNetCore.Mvc;
using TimeTrackerWeb.Data;
using TimeTrackerWeb.Domain;

namespace TimeTrackerWeb.Controllers;

public class TasksController : Controller
{
    private readonly AppDbContext _db;
    public TasksController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Create(int projectId, string title)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            _db.Tasks.Add(new TaskItem
            {
                ProjectId = projectId,
                Title = title.Trim(),
                IsActive = true
            });
            await _db.SaveChangesAsync();
        }

        return RedirectToAction("Index", "Dashboard", new { projectId = projectId });
    }
}