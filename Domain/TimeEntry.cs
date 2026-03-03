namespace TimeTrackerWeb.Domain;

public class TimeEntry
{
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public DateTime StartTimeUtc { get; set; }
    public DateTime? EndTimeUtc { get; set; }

    public TaskItem? TaskItem { get; set; }

    public long DurationSeconds =>
        (long)(((EndTimeUtc ?? DateTime.UtcNow) - StartTimeUtc).TotalSeconds);
}