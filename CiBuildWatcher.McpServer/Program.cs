using CiBuildWatcher.McpServer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace CiBuildWatcher.McpServer;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // MCP servers write logs to stderr; this matches the SDK docs
        builder.Logging.AddConsole(consoleOptions =>
        {
            consoleOptions.LogToStandardErrorThreshold = LogLevel.Trace;
        });

        // Register our fake CI data service
        builder.Services.AddSingleton<FakeCiDataService>();

        // Register MCP server with stdio transport and auto-discovered tools
        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()      // server communicates over stdin/stdout
            .WithToolsFromAssembly();        // find [McpServerTool]s in this assembly

        var host = builder.Build();

        Console.Error.WriteLine("🚀 CiBuildWatcher MCP server starting (stdio)…");
        await host.RunAsync();
    }
}
