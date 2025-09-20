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
                RhinoMcpServer.Start();
                // Consider removing this next line or change to “starting…”
                RhinoApp.WriteLine("[MCP] Starting HTTP server…");
            }
            else
            {
                RhinoApp.WriteLine("[MCP] Server already running.");
            }
            return Result.Success;
        }
    }
}
