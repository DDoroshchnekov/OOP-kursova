using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeTrackerWeb.Data;
using TimeTrackerWeb.Domain;

namespace TimeTrackerWeb.Controllers;

public class ProjectsController : Controller
{
    private readonly AppDbContext _db;
    public ProjectsController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Create(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            _db.Projects.Add(new Project { Name = name.Trim() });
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Index", "Dashboard");
    }
}