using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.AspNetCore;   // MapMcp()
using ModelContextProtocol.Server;       // AddMcpServer(), WithToolsFromAssembly()
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace throughline.plugin.Infrastructure
{
    /// <summary>
    /// Starts an HTTP/SSE MCP server (Kestrel) inside the Rhino process.
    /// </summary>
    public static class RhinoMcpServer
    {
        private static readonly object _gate = new();
        private static CancellationTokenSource? _cts;
        private static Task? _serverTask;
        private static int _port = 8765; // fixed local port
        private static string _host = "127.0.0.1";

        public static bool IsRunning
        {
            get { lock (_gate) return _serverTask is { IsCompleted: false }; }
        }

        public static string BaseUrl => $"http://{_host}:{_port}";

        public static void Start(string? host = null, int? port = null)
        {
            lock (_gate)
            {
                if (IsRunning) return;

                if (!string.IsNullOrWhiteSpace(host)) _host = host!;
                if (port is { } p) _port = p;

                _cts = new CancellationTokenSource();

                _serverTask = Task.Run(async () =>
                {
                    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
                    {
                        Args = Array.Empty<string>()
                    });

                    builder.Logging.ClearProviders();
                    builder.Logging.AddSimpleConsole(o => o.IncludeScopes = false);

                    // Register MCP + discover tools in this assembly
                    builder.Services
                        .AddMcpServer()
                        // .WithHttpTransport() // uncomment if your preview requires it
                        .WithToolsFromAssembly(Assembly.GetExecutingAssembly());

                    // Configure Kestrel explicitly (avoid UseUrls)
                    builder.WebHost.ConfigureKestrel(options =>
                    {
                        options.ListenLocalhost(_port, listenOptions =>
                        {
                            listenOptions.Protocols = HttpProtocols.Http1; // SSE uses HTTP/1
                        });
                    });

                    var app = builder.Build();

                    // Simple health check (no Results helper needed)
                    app.MapGet("/", () => new { status = "ok", server = "Rhino MCP" });

                    // Expose MCP HTTP/SSE endpoints (e.g., /sse and /messages)
                    app.MapMcp();

                    await app.RunAsync(_cts!.Token);
                }, _cts.Token);
            }
        }

        public static async Task StopAsync()
        {
            Task? running;
            lock (_gate)
            {
                running = _serverTask;
                if (running == null) return;
                _cts?.Cancel();
            }

            try { await running!; } catch { /* ignore during shutdown */ }

            lock (_gate)
            {
                _serverTask = null;
                _cts?.Dispose();
                _cts = null;
            }
        }
    }
}
