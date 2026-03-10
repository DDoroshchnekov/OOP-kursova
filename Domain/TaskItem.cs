namespace TimeTrackerWeb.Domain
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;

        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        public bool IsActive { get; set; } = false;

        public string UserId { get; set; } = string.Empty;

        public List<TimeEntry> TimeEntries { get; set; } = new();
    }
}