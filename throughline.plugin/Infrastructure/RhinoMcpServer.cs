using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Server;
using Rhino;
using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace throughline.plugin.Infrastructure
{
    public static class RhinoMcpServer
    {
        private static readonly object _gate = new();
        private static CancellationTokenSource? _cts;
        private static Task? _serverTask;
        private static int _port = 8765;
        private static string _host = "localhost";

        public static bool IsRunning
        {
            get { lock (_gate) return _serverTask is { IsCompleted: false }; }
        }

        public static string BaseUrl => $"http://{_host}:{_port}";

        public static void Start(string? host = null, int? port = null)
        {
            lock (_gate)
            {
                if (IsRunning)
                {
                    RhinoApp.WriteLine("[MCP] Already running at " + BaseUrl);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(host)) _host = host!;
                if (port is int p) _port = p;

                _cts = new CancellationTokenSource();

                //_serverTask = Task.Run(async () =>
                //{
                    try
                    {
                        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = Array.Empty<string>() });

                        // Logging to Rhino console
                        builder.Logging.ClearProviders();
                        builder.Logging.AddSimpleConsole(o => o.IncludeScopes = false);

                        builder.Services
                            .AddMcpServer()
                            // If your preview requires explicit HTTP transport, uncomment:
                            // .WithHttpTransport()
                            .WithToolsFromAssembly(Assembly.GetExecutingAssembly());

                        // Force Kestrel + explicit bind; avoid UseUrls ambiguity in class libs
                        builder.WebHost
                            .UseKestrel()
                            .ConfigureKestrel(options =>
                            {
                                // Bind explicitly to IPv4 loopback
                                options.Listen(IPAddress.Loopback, _port, listen =>
                                {
                                    listen.Protocols = HttpProtocols.Http1;
                                });

                                // And to IPv6 loopback (so localhost/::1 also work)
                                options.Listen(IPAddress.IPv6Loopback, _port, listen =>
                                {
                                    listen.Protocols = HttpProtocols.Http1;
                                });

                                // If you prefer “listen everywhere” instead, use:
                                // options.ListenAnyIP(_port, listen => listen.Protocols = HttpProtocols.Http1);
                            });

                        var app = builder.Build();

                        // Optional: simple health
                        app.MapGet("/", () => new { status = "ok", server = "Rhino MCP" });

                        // MCP HTTP/SSE endpoints (/sse, /messages)
                        app.MapMcp();

                        // Announce once the server is really listening
                        var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
                        lifetime.ApplicationStarted.Register(() =>
                        {
                            RhinoApp.WriteLine($"[MCP] Listening on {BaseUrl} (endpoints: /sse, /messages)");
                        });

                        app.RunAsync(_cts!.Token);
                        //await app.RunAsync(_cts!.Token);
                        RhinoApp.WriteLine($"[MCP] exited");
                    }
                    catch (Exception ex)
                    {
                        RhinoApp.WriteLine("[MCP] Host failed to start:");
                        RhinoApp.WriteLine(ex.ToString());
                    }
                //}, _cts.Token);
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

            try { await running!; } catch { /* expected on cancel */ }

            lock (_gate)
            {
                _serverTask = null;
                _cts?.Dispose();
                _cts = null;
            }

            RhinoApp.WriteLine("[MCP] Server stopped.");
        }
    }
}
