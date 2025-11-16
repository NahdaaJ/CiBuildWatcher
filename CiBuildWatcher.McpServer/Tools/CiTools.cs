using System.ComponentModel;
using System.Text;
using CiBuildWatcher.McpServer.Services;
using ModelContextProtocol.Server;
using System.Linq;

namespace CiBuildWatcher.McpServer.Tools;

[McpServerToolType]
public static class CiTools
{
    // Tool 1: List all repos
    [McpServerTool(Name = "list_repos")]
    [Description("Lists all repositories and their last build status.")]
    public static string ListRepos(FakeCiDataService data)
    {
        var sb = new StringBuilder();
        sb.AppendLine("📦 Repositories");
        sb.AppendLine("---------------------------");

        foreach (var repo in data.GetRepos())
        {
            sb.AppendLine(
                $"{repo.Name} | Last build: {repo.LastBuildAt:g} | Status: {repo.LastBuildStatus}");
        }

        return sb.ToString();
    }

    // Tool 2: List builds for a given repo
    [McpServerTool(Name = "list_builds")]
    [Description("Lists recent builds for the given repository.")]
    public static string ListBuilds(
        FakeCiDataService data,
        [Description("Name of the repository")] string repoName)
    {
        var builds = data.GetBuildsForRepo(repoName).ToList();
        if (!builds.Any())
        {
            return $"No builds found for repo '{repoName}'.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"🛠 Builds for {repoName}");
        sb.AppendLine("---------------------------");

        foreach (var build in builds.OrderByDescending(b => b.BuildNumber))
        {
            sb.AppendLine(
                $"#{build.BuildNumber} | {build.Status} | Started: {build.StartedAt:g} | Finished: {build.FinishedAt:g}");
        }

        return sb.ToString();
    }

    // Tool 3: Find repos not built in X days
    [McpServerTool(Name = "get_stale_repos")]
    [Description("Finds repositories that haven't had a build in the last N days (default 14).")]
    public static string GetStaleRepos(
        FakeCiDataService data,
        [Description("Number of days without builds to consider stale")] int days = 14)
    {
        var stale = data.GetStaleRepos(days).ToList();
        if (!stale.Any())
        {
            return $"No repositories are stale (> {days} days without a build).";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"⚠️ Stale repositories (no builds in > {days} days)");
        sb.AppendLine("---------------------------");

        foreach (var repo in stale)
        {
            sb.AppendLine($"{repo.Name} | Last build: {repo.LastBuildAt:g} | Status: {repo.LastBuildStatus}");
        }

        return sb.ToString();
    }

    [McpServerTool(Name = "get_repo_status")]
    [Description("Returns a short summary of the health of a single repository.")]
    public static string GetRepoStatus(
    FakeCiDataService data,
    [Description("Name of the repository")] string repoName)
    {
        var repo = data.GetRepos()
            .FirstOrDefault(r => string.Equals(r.Name, repoName, StringComparison.OrdinalIgnoreCase));

        if (repo is null)
        {
            return $"Repository '{repoName}' not found.";
        }

        var builds = data.GetBuildsForRepo(repoName)
            .OrderByDescending(b => b.BuildNumber)
            .ToList();

        var latestBuild = builds.FirstOrDefault();
        var failedCount = builds.Count(b => !string.Equals(b.Status, "Success", StringComparison.OrdinalIgnoreCase));

        var staleCutoff = DateTime.UtcNow.AddDays(-14);
        var isStale = repo.LastBuildAt < staleCutoff;

        return
            $"📦 {repo.Name}\n" +
            $"- Last build: {repo.LastBuildAt:g} ({repo.LastBuildStatus})\n" +
            $"- Builds tracked: {builds.Count}, failures: {failedCount}\n" +
            $"- Stale (>14 days without build): {(isStale ? "YES" : "no")}";
    }

    [McpServerTool(Name = "get_failed_builds")]
    [Description("Lists all failed builds in the last N days across all repositories (default 30).")]
    public static string GetFailedBuilds(
    FakeCiDataService data,
    [Description("How many days back to look for failures.")] int days = 30)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);

        var failed = data.GetRepos()
            .SelectMany(r => r.Builds)
            .Where(b => !string.Equals(b.Status, "Success", StringComparison.OrdinalIgnoreCase)
                        && b.StartedAt >= cutoff)
            .OrderByDescending(b => b.StartedAt)
            .ToList();

        if (!failed.Any())
        {
            return $"No failed builds in the last {days} days. 🎉";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"❌ Failed builds in the last {days} days");
        sb.AppendLine("--------------------------------------");

        foreach (var b in failed)
        {
            sb.AppendLine(
                $"{b.RepoName} | Build #{b.BuildNumber} | {b.Status} | Started: {b.StartedAt:g} | Finished: {b.FinishedAt:g}");
        }

        return sb.ToString();
    }

    [McpServerTool(Name = "get_repo_failure_rate")]
    [Description("Shows the success/failure rate for a single repository over its last N builds.")]
    public static string GetRepoFailureRate(
    FakeCiDataService data,
    [Description("Name of the repository.")] string repoName,
    [Description("How many recent builds to analyse.")] int lastN = 10)
    {
        var builds = data.GetBuildsForRepo(repoName)
            .OrderByDescending(b => b.BuildNumber)
            .Take(lastN)
            .ToList();

        if (!builds.Any())
        {
            return $"No builds found for repo '{repoName}'.";
        }

        var total = builds.Count;
        var failures = builds.Count(b => !string.Equals(b.Status, "Success", StringComparison.OrdinalIgnoreCase));
        var successes = total - failures;
        var failureRate = total > 0 ? (double)failures / total * 100 : 0;

        var sb = new StringBuilder();
        sb.AppendLine($"📊 Failure rate for {repoName} (last {total} builds)");
        sb.AppendLine("--------------------------------------------");
        sb.AppendLine($"Successes: {successes}");
        sb.AppendLine($"Failures:  {failures}");
        sb.AppendLine($"Failure rate: {failureRate:F1}%");

        return sb.ToString();
    }

    [McpServerTool(Name = "get_build_health_overview")]
    [Description("High-level CI health summary: repo counts, failures, and stale services.")]
    public static string GetBuildHealthOverview(
    FakeCiDataService data,
    [Description("Repos with no builds in more than this many days are considered stale.")] int staleDays = 14)
    {
        var repos = data.GetRepos().ToList();
        var totalRepos = repos.Count;

        var allBuilds = repos.SelectMany(r => r.Builds).ToList();
        var totalBuilds = allBuilds.Count;

        var failedBuilds = allBuilds
            .Where(b => !string.Equals(b.Status, "Success", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var staleRepos = data.GetStaleRepos(staleDays).ToList();

        var sb = new StringBuilder();
        sb.AppendLine("📈 CI build health overview");
        sb.AppendLine("---------------------------");
        sb.AppendLine($"Total repositories: {totalRepos}");
        sb.AppendLine($"Total builds tracked: {totalBuilds}");
        sb.AppendLine($"Failed builds: {failedBuilds.Count}");
        sb.AppendLine($"Stale repos (no builds in > {staleDays} days): {staleRepos.Count}");
        sb.AppendLine();

        // Top 3 repos by number of failures
        var topFailureRepos = failedBuilds
            .GroupBy(b => b.RepoName)
            .Select(g => new { RepoName = g.Key, Failures = g.Count() })
            .OrderByDescending(x => x.Failures)
            .Take(3)
            .ToList();

        if (topFailureRepos.Any())
        {
            sb.AppendLine("🔥 Repos with the most failures:");
            foreach (var r in topFailureRepos)
            {
                sb.AppendLine($"- {r.RepoName}: {r.Failures} failed builds");
            }
        }
        else
        {
            sb.AppendLine("No failing builds detected in the current history. ✔");
        }

        return sb.ToString();
    }

    [McpServerTool(Name = "get_flaky_repos")]
    [Description("Finds repos that have both successful and failed builds in the last N days (potentially flaky).")]
    public static string GetFlakyRepos(
    FakeCiDataService data,
    [Description("How many days back to look at build history.")] int days = 30)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);

        var repos = data.GetRepos().ToList();
        var flaky = new List<(string RepoName, int Successes, int Failures)>();

        foreach (var repo in repos)
        {
            var recent = repo.Builds
                .Where(b => b.StartedAt >= cutoff)
                .ToList();

            if (!recent.Any()) continue;

            var successes = recent.Count(b => string.Equals(b.Status, "Success", StringComparison.OrdinalIgnoreCase));
            var failures = recent.Count - successes;

            if (successes > 0 && failures > 0)
            {
                flaky.Add((repo.Name, successes, failures));
            }
        }

        if (!flaky.Any())
        {
            return $"No flaky repos detected in the last {days} days.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"⚠️ Flaky repos in the last {days} days");
        sb.AppendLine("-------------------------------------");

        foreach (var r in flaky.OrderByDescending(f => f.Failures))
        {
            sb.AppendLine($"- {r.RepoName}: {r.Successes} successes, {r.Failures} failures");
        }

        return sb.ToString();
    }


}
