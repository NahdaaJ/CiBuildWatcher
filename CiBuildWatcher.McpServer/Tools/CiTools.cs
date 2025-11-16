using System.ComponentModel;
using System.Text;
using CiBuildWatcher.McpServer.Services;
using ModelContextProtocol.Server;

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
}
