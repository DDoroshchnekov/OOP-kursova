namespace TimeTrackerWeb.Domain
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public List<TaskItem> Tasks { get; set; } = new();
    }
}