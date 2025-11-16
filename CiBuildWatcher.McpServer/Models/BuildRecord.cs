using System.Text.Json.Serialization;

namespace CiBuildWatcher.McpServer.Models;

public class BuildRecord
{
    // Filled in by FakeCiDataService after load; not read from JSON
    [JsonIgnore]
    public string RepoName { get; set; } = string.Empty;

    public int BuildNumber { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public string Status { get; set; } = "Unknown";
}
