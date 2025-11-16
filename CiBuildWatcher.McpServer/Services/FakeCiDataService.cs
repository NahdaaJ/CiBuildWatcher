using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CiBuildWatcher.McpServer.Models;

namespace CiBuildWatcher.McpServer.Services;

public class FakeCiDataService
{
    private readonly List<CiRepository> _repos;
    private readonly List<BuildRecord> _builds;

    public FakeCiDataService()
    {
        // Simple seeded data for the demo
        _repos = new List<CiRepository>
        {
            new()
            {
                Name = "PaymentService",
                LastBuildAt = DateTime.UtcNow.AddDays(-1),
                LastBuildStatus = "Success"
            },
            new()
            {
                Name = "UserPortal",
                LastBuildAt = DateTime.UtcNow.AddDays(-10),
                LastBuildStatus = "Failed"
            },
            new()
            {
                Name = "ReportingJob",
                LastBuildAt = DateTime.UtcNow.AddDays(-21),
                LastBuildStatus = "Success"
            }
        };

        _builds = new List<BuildRecord>
        {
            new()
            {
                RepoName = "PaymentService",
                BuildNumber = 102,
                StartedAt = DateTime.UtcNow.AddDays(-1).AddMinutes(-10),
                FinishedAt = DateTime.UtcNow.AddDays(-1),
                Status = "Success"
            },
            new()
            {
                RepoName = "UserPortal",
                BuildNumber = 58,
                StartedAt = DateTime.UtcNow.AddDays(-10).AddMinutes(-20),
                FinishedAt = DateTime.UtcNow.AddDays(-10),
                Status = "Failed"
            },
            new()
            {
                RepoName = "ReportingJob",
                BuildNumber = 12,
                StartedAt = DateTime.UtcNow.AddDays(-21).AddMinutes(-30),
                FinishedAt = DateTime.UtcNow.AddDays(-21),
                Status = "Success"
            }
        };
    }

    public IEnumerable<CiRepository> GetRepos() => _repos;

    public IEnumerable<BuildRecord> GetBuildsForRepo(string repoName) =>
        _builds.Where(b => string.Equals(b.RepoName, repoName, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<CiRepository> GetStaleRepos(int daysThreshold)
    {
        var cutoff = DateTime.UtcNow.AddDays(-daysThreshold);
        return _repos.Where(r => r.LastBuildAt < cutoff);
    }
}
