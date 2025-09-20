using Rhino;
using Rhino.Commands;
using throughline.plugin.Infrastructure;

namespace throughline.plugin
{
    // Start server
    public class ThroughlineStartCommand : Command
    {
        public static ThroughlineStartCommand Instance { get; private set; }
        public ThroughlineStartCommand() => Instance = this;
        public override string EnglishName => "ThroughlineStart";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            if (!RhinoMcpServer.IsRunning)
            {
                // You can pass a custom URL/port here if you prefer:
                // RhinoMcpServer.Start("http://127.0.0.1:9009");
                RhinoMcpServer.Start();
                RhinoApp.WriteLine($"[MCP] HTTP server started at {RhinoMcpServer.BaseUrl} (endpoints: /sse, /messages)");
            }
            else
            {
                RhinoApp.WriteLine("[MCP] Server already running.");
            }
            return Result.Success;
        }
    }
}
