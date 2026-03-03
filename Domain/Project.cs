namespace TimeTrackerWeb.Domain;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<TaskItem> Tasks { get; set; } = new();
}