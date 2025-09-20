using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace throughline.plugin
{
    public class ThroughLineStopCommand : Command
    {
        public ThroughLineStopCommand() => Instance = this;
        public static ThroughLineStopCommand Instance { get; private set; }
        public override string EnglishName => "tlStop";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            Task.Run(() => ThroughlineMcpServer.StopAsync());
            RhinoApp.WriteLine("Throughline MCP server: stopping…");
            return Result.Success;
        }
    }

}
