namespace TimeTrackerWeb.Domain;

public class TaskItem
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Title { get; set; } = "";
    public bool IsActive { get; set; } = true;

    public Project? Project { get; set; }
    public List<TimeEntry> TimeEntries { get; set; } = new();
}