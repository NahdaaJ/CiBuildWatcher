namespace CiBuildWatcher.McpServer.Models;

public class CiRepository
{
    public string Name { get; set; } = string.Empty;
    public DateTime LastBuildAt { get; set; }
    public string LastBuildStatus { get; set; } = "Unknown";

    public List<BuildRecord> Builds { get; set; } = new();
}

