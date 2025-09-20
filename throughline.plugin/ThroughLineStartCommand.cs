using Rhino;
using Rhino.Commands;
using System.Threading.Tasks;

namespace throughline.plugin
{
    public class ThroughLineStartCommend : Command
    {
        public ThroughLineStartCommend() => Instance = this;
        public static ThroughLineStartCommend Instance { get; private set; }
        public override string EnglishName => "tlStart";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // Start the MCP server host in the background
            Task.Run(() => ThroughlineMcpServer.StartAsync());

            RhinoApp.WriteLine("Throughline MCP server: started (STDIO).");
            return Result.Success;
        }
    }
}