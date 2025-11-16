using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using System.IO;

namespace CiBuildWatcher.CliDemo;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("🔌 Starting CiBuildWatcher CLI demo…");

        // Figure out the solution root from the current executable location
        var baseDir = AppContext.BaseDirectory;
        // ...\CiBuildWatcher.CliDemo\bin\Debug\net8.0\
        var solutionRoot = Path.GetFullPath(
            Path.Combine(baseDir, "..", "..", "..", "..")); // go up 4 levels

        var serverProjectPath = Path.Combine(
            solutionRoot,
            "CiBuildWatcher.McpServer",
            "CiBuildWatcher.McpServer.csproj");

        Console.WriteLine($"Server project path: {serverProjectPath}");

        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "CiBuildWatcherDemo",
            Command = "dotnet",
            Arguments = new[] { "run", "--project", serverProjectPath }
        });

        var client = await McpClient.CreateAsync(transport);

        Console.WriteLine("✅ Connected to MCP server.");
        Console.WriteLine();

        // 1) List tools
        Console.WriteLine("🧰 Available tools:");
        var tools = await client.ListToolsAsync();
        foreach (var tool in tools)
        {
            Console.WriteLine($"- {tool.Name}: {tool.Description}");
        }

        Console.WriteLine();
        Console.WriteLine("▶ Demo: list_repos");
        await CallAndPrintToolAsync(client, "list_repos");

        Console.WriteLine();
        Console.WriteLine("▶ Demo: list_builds for 'PaymentService'");
        await CallAndPrintToolAsync(client, "list_builds", new Dictionary<string, object?>
        {
            ["repoName"] = "PaymentService"
        });

        Console.WriteLine();
        Console.WriteLine("▶ Demo: get_stale_repos (default 14 days)");
        await CallAndPrintToolAsync(client, "get_stale_repos");

        Console.WriteLine();
        Console.WriteLine("🎉 Demo complete. Press any key to exit.");
        Console.ReadKey();
    }

    private static async Task CallAndPrintToolAsync(
        McpClient client,
        string toolName,
        Dictionary<string, object?>? args = null)
    {
        var result = await client.CallToolAsync(
            toolName,
            args ?? new Dictionary<string, object?>(),
            cancellationToken: CancellationToken.None);

        // In our server, tools return plain strings so we expect a single text block
        var textBlock = result.Content
        .OfType<TextContentBlock>()
        .FirstOrDefault();

        if (textBlock is null)
        {
            Console.WriteLine("[No text content returned]");
        }
        else
        {
            Console.WriteLine(textBlock.Text);
        }
    }
}
