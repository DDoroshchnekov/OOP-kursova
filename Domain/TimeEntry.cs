namespace TimeTrackerWeb.Domain
{
    public class TimeEntry
    {
        public int Id { get; set; }

        public int TaskItemId { get; set; }
        public TaskItem? TaskItem { get; set; }

        public DateTime StartTimeUtc { get; set; }
        public DateTime? EndTimeUtc { get; set; }

        public int DurationSeconds { get; set; }

        public string UserId { get; set; } = string.Empty;
    }
}