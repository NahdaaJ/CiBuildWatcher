using System.Text.Json;
using CiBuildWatcher.McpServer.Models;

namespace CiBuildWatcher.McpServer.Services;

public class FakeCiDataService
{
    private readonly List<CiRepository> _repos;
    private readonly List<BuildRecord> _builds;

    public FakeCiDataService()
    {
        // Try to load from JSON; if anything fails, fall back to in-memory seed data
        try
        {
            var baseDir = AppContext.BaseDirectory; // ...\bin\Debug\net8.0\
            var jsonPath = Path.Combine(baseDir, "Data", "ci_data.json");

            Console.Error.WriteLine($"[FakeCiDataService] Attempting to load CI data from: {jsonPath}");

            if (File.Exists(jsonPath))
            {
                var json = File.ReadAllText(jsonPath);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var root = JsonSerializer.Deserialize<RootCiData>(json, options);

                if (root?.Repositories != null && root.Repositories.Count > 0)
                {
                    _repos = root.Repositories;
                    _builds = new List<BuildRecord>();

                    foreach (var repo in _repos)
                    {
                        if (repo.Builds == null) continue;

                        foreach (var build in repo.Builds)
                        {
                            build.RepoName = repo.Name; // stamp repo name on each build
                            _builds.Add(build);
                        }
                    }

                    Console.Error.WriteLine($"[FakeCiDataService] Loaded {_repos.Count} repositories and {_builds.Count} builds from JSON.");
                    return;
                }

                Console.Error.WriteLine("[FakeCiDataService] JSON loaded but no repositories found; falling back to default in-memory data.");
            }
            else
            {
                Console.Error.WriteLine($"[FakeCiDataService] JSON file not found at {jsonPath}; falling back to default in-memory data.");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[FakeCiDataService] Exception while loading JSON: {ex.Message}");
            Console.Error.WriteLine("[FakeCiDataService] Falling back to default in-memory data.");
        }

        // === FALLBACK: in-memory seeded data (mirrors the JSON shape) ===

        var paymentService = new CiRepository
        {
            Name = "PaymentService",
            LastBuildAt = DateTime.UtcNow.AddDays(-1),
            LastBuildStatus = "Success"
        };
        paymentService.Builds.AddRange(new[]
        {
            new BuildRecord
            {
                RepoName = paymentService.Name,
                BuildNumber = 102,
                StartedAt = DateTime.UtcNow.AddDays(-1).AddMinutes(-10),
                FinishedAt = DateTime.UtcNow.AddDays(-1),
                Status = "Success"
            },
            new BuildRecord
            {
                RepoName = paymentService.Name,
                BuildNumber = 101,
                StartedAt = DateTime.UtcNow.AddDays(-5).AddMinutes(-5),
                FinishedAt = DateTime.UtcNow.AddDays(-5),
                Status = "Success"
            }
        });

        var userPortal = new CiRepository
        {
            Name = "UserPortal",
            LastBuildAt = DateTime.UtcNow.AddDays(-9),
            LastBuildStatus = "Failed"
        };
        userPortal.Builds.AddRange(new[]
        {
            new BuildRecord
            {
                RepoName = userPortal.Name,
                BuildNumber = 58,
                StartedAt = DateTime.UtcNow.AddDays(-9).AddMinutes(-20),
                FinishedAt = DateTime.UtcNow.AddDays(-9),
                Status = "Failed"
            },
            new BuildRecord
            {
                RepoName = userPortal.Name,
                BuildNumber = 57,
                StartedAt = DateTime.UtcNow.AddDays(-11).AddMinutes(-15),
                FinishedAt = DateTime.UtcNow.AddDays(-11),
                Status = "Success"
            }
        });

        var reportingJob = new CiRepository
        {
            Name = "ReportingJob",
            LastBuildAt = DateTime.UtcNow.AddDays(-20),
            LastBuildStatus = "Success"
        };
        reportingJob.Builds.Add(new BuildRecord
        {
            RepoName = reportingJob.Name,
            BuildNumber = 12,
            StartedAt = DateTime.UtcNow.AddDays(-20).AddMinutes(-30),
            FinishedAt = DateTime.UtcNow.AddDays(-20),
            Status = "Success"
        });

        var inventoryApi = new CiRepository
        {
            Name = "InventoryApi",
            LastBuildAt = DateTime.UtcNow.AddDays(-3),
            LastBuildStatus = "Success"
        };
        inventoryApi.Builds.AddRange(new[]
        {
            new BuildRecord
            {
                RepoName = inventoryApi.Name,
                BuildNumber = 88,
                StartedAt = DateTime.UtcNow.AddDays(-3).AddMinutes(-15),
                FinishedAt = DateTime.UtcNow.AddDays(-3),
                Status = "Success"
            },
            new BuildRecord
            {
                RepoName = inventoryApi.Name,
                BuildNumber = 87,
                StartedAt = DateTime.UtcNow.AddDays(-7).AddMinutes(-10),
                FinishedAt = DateTime.UtcNow.AddDays(-7),
                Status = "Failed"
            }
        });

        var emailDispatcher = new CiRepository
        {
            Name = "EmailDispatcher",
            LastBuildAt = DateTime.UtcNow.AddDays(-25),
            LastBuildStatus = "Failed"
        };
        emailDispatcher.Builds.Add(new BuildRecord
        {
            RepoName = emailDispatcher.Name,
            BuildNumber = 33,
            StartedAt = DateTime.UtcNow.AddDays(-25).AddMinutes(-20),
            FinishedAt = DateTime.UtcNow.AddDays(-25),
            Status = "Failed"
        });

        var analyticsEngine = new CiRepository
        {
            Name = "AnalyticsEngine",
            LastBuildAt = DateTime.UtcNow.AddDays(-13),
            LastBuildStatus = "Success"
        };
        analyticsEngine.Builds.Add(new BuildRecord
        {
            RepoName = analyticsEngine.Name,
            BuildNumber = 145,
            StartedAt = DateTime.UtcNow.AddDays(-13).AddMinutes(-15),
            FinishedAt = DateTime.UtcNow.AddDays(-13),
            Status = "Success"
        });

        var notificationHub = new CiRepository
        {
            Name = "NotificationHub",
            LastBuildAt = DateTime.UtcNow.AddDays(-18),
            LastBuildStatus = "Failed"
        };
        notificationHub.Builds.Add(new BuildRecord
        {
            RepoName = notificationHub.Name,
            BuildNumber = 22,
            StartedAt = DateTime.UtcNow.AddDays(-18).AddMinutes(-30),
            FinishedAt = DateTime.UtcNow.AddDays(-18),
            Status = "Failed"
        });

        //_repos = new List<CiRepository>
        //{
        //    paymentService,
        //    userPortal,
        //    reportingJob,
        //    inventoryApi,
        //    emailDispatcher,
        //    analyticsEngine,
        //    notificationHub
        //};

        _repos = [];

        _builds = _repos
            .SelectMany(r => r.Builds)
            .ToList();

        Console.Error.WriteLine($"[FakeCiDataService] Using fallback data: {_repos.Count} repositories, {_builds.Count} builds.");
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

// Shape of the JSON root object
public class RootCiData
{
    public List<CiRepository>? Repositories { get; set; }
}
