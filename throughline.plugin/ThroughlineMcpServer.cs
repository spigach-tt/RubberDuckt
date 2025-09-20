// ThroughlineMcpServer.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace throughline.plugin
{
    /// <summary>
    /// Manages the lifetime of the MCP server inside the Rhino process.
    /// </summary>
    internal static class ThroughlineMcpServer
    {
        private static IHost _host;
        private static CancellationTokenSource _cts;

        public static bool IsRunning => _host != null;

        public static async Task StartAsync()
        {
            if (_host != null) return;

            _cts = new CancellationTokenSource();

            var builder = Host.CreateApplicationBuilder(Array.Empty<string>());

            // IMPORTANT: log to stderr (NOT stdout) for STDIO transport.
            builder.Logging.AddConsole(options =>
            {
                options.LogToStandardErrorThreshold = LogLevel.Trace;
            });

            builder.Services
                .AddMcpServer()
                .WithStdioServerTransport()   // STDIO transport
                .WithToolsFromAssembly();     // discovers [McpServerToolType]/[McpServerTool]

            _host = builder.Build();

            // fire-and-forget background run
            _ = _host.RunAsync(_cts.Token);
        }

        public static async Task StopAsync()
        {
            if (_host == null) return;

            _cts?.Cancel();
            try { await _host.StopAsync(); } catch { /* ignore */ }
            _host.Dispose();
            _host = null;
            _cts?.Dispose();
            _cts = null;
        }
    }
}
