
// Stop server
using Rhino;
using Rhino.Commands;
using throughline.plugin.Infrastructure;

namespace throughline.plugin
{
    public class ThroughlineStopCommand : Command
    {
        public static ThroughlineStopCommand Instance { get; private set; }
        public ThroughlineStopCommand() => Instance = this;
        public override string EnglishName => "ThroughlineStop";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            RhinoMcpServer.StopAsync().GetAwaiter().GetResult();
            RhinoApp.WriteLine("[MCP] Server stopped.");
            return Result.Success;
        }
    }
}