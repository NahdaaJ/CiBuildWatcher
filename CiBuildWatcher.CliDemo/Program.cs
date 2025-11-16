using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using CiBuildWatcher.CliDemo.Llm;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;



namespace CiBuildWatcher.CliDemo;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("> Starting CiBuildWatcher CLI demo…");

        // Figure out the solution root from the current executable location
        var baseDir = AppContext.BaseDirectory;
        // ...\CiBuildWatcher.CliDemo\bin\Debug\net8.0\
        var solutionRoot = Path.GetFullPath(
            Path.Combine(baseDir, "..", "..", "..", "..")); // go up 4 levels

        var serverProjectPath = Path.Combine(
            solutionRoot,
            "CiBuildWatcher.McpServer",
            "CiBuildWatcher.McpServer.csproj");

        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "CiBuildWatcherDemo",
            Command = "dotnet",
            Arguments = new[] { "run", "--project", serverProjectPath }
        });

        var client = await McpClient.CreateAsync(transport);

        Console.WriteLine("> Connected to MCP server.");
        Console.WriteLine();

        // 1) List tools
        Console.WriteLine("----------------------- AVAILABLE TOOLS -----------------------");
        var tools = await client.ListToolsAsync();
        foreach (var tool in tools)
        {
            Console.WriteLine($"- {tool.Name}: {tool.Description}");
        }

        Console.ReadKey();

        Console.WriteLine();
        Console.WriteLine(">>>> Demo: list_repos");
        await CallAndPrintToolAsync(client, "list_repos");
        Console.ReadKey();

        Console.WriteLine();
        Console.WriteLine(">>>> Demo: list_builds for repo 'PaymentService'");
        await CallAndPrintToolAsync(client, "list_builds", new Dictionary<string, object?>
        {
            ["repoName"] = "PaymentService"
        });
        Console.ReadKey();

        Console.WriteLine();
        Console.WriteLine(">>>> Demo: get_repo_status for repo 'UserPortal'");
        await CallAndPrintToolAsync(client, "get_repo_status", new Dictionary<string, object?>
        {
            ["repoName"] = "UserPortal"
        });
        Console.ReadKey();

        Console.WriteLine();
        Console.WriteLine(">>>> Demo: get_stale_repos (default 14 days)");
        var staleText = await CallAndPrintToolAsync(client, "get_stale_repos");
        Console.ReadKey();

        Console.WriteLine();
        Console.WriteLine("~ LLM summary (Ollama / llama3.2):");

        try
        {
            var summary = await SummariseWithOllamaAsync(staleText);
            Console.WriteLine(summary);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Could not contact Ollama: {ex.Message}]");
        }

        Console.ReadKey();

        Console.WriteLine();
        Console.WriteLine("----------------------- EXTRA TOOLS -----------------------\n");
        Console.WriteLine("> Demo: get_failed_builds (last 30 days)");
        await CallAndPrintToolAsync(client, "get_failed_builds", new Dictionary<string, object?>
        {
            ["days"] = 30
        });
        Console.ReadKey();

        Console.WriteLine();
        Console.WriteLine("> Demo: get_repo_failure_rate for 'InventoryApi'");
        await CallAndPrintToolAsync(client, "get_repo_failure_rate", new Dictionary<string, object?>
        {
            ["repoName"] = "InventoryApi",
            ["lastN"] = 10
        });
        Console.ReadKey();

        Console.WriteLine();
        Console.WriteLine("> Demo: get_flaky_repos (last 30 days)");
        await CallAndPrintToolAsync(client, "get_flaky_repos", new Dictionary<string, object?>
        {
            ["days"] = 30
        });
        Console.ReadKey();

        Console.WriteLine();
        Console.WriteLine("> Demo: get_build_health_overview");
        var overviewText = await CallAndPrintToolAsync(client, "get_build_health_overview");
        Console.ReadKey();

        Console.WriteLine();
        Console.WriteLine("~ LLM summary (Ollama / llama3.2):");

        try
        {
            var summary = await SummariseWithOllamaAsync(overviewText);
            Console.WriteLine(summary);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Could not contact Ollama: {ex.Message}]");
        }

        Console.WriteLine();
        Console.WriteLine(">> Demo complete! Press any key to exit.");
        Console.ReadKey();
    }

    private static async Task<string> CallAndPrintToolAsync(
        McpClient client,
        string toolName,
        Dictionary<string, object?>? args = null)
    {
        var result = await client.CallToolAsync(
            toolName,
            args ?? new Dictionary<string, object?>(),
            cancellationToken: CancellationToken.None);

        // We expect text content from our MCP tools
        var textBlock = result.Content
            .OfType<TextContentBlock>()
            .FirstOrDefault();

        var text = textBlock?.Text ?? "[No text content returned]";
        Console.WriteLine(text);
        return text;
    }

    private static async Task<string> SummariseWithOllamaAsync(string ciText)
    {
        using var http = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:11434")
        };

        var payload = new
        {
            model = "llama3.2",   // the model you pulled
            stream = false,
            messages = new[]
            {
            new { role = "system", content = "You are a helpful DevOps assistant." },
            new { role = "user", content = $"Summarise this CI/CD status in 2 bullet points:\n{ciText}." +
            $"\nExplain to the user that they should run the pipeline again, and describe methods " +
            $"(assuming its azure ado) to make sure the build is run frequently." }
        }
        };

        var response = await http.PostAsJsonAsync("/api/chat", payload);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<OllamaChatResponse>();
        return data?.Message?.Content ?? "[No summary returned]";
    }


}
