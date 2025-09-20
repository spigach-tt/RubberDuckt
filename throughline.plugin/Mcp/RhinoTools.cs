using System;
using System.ComponentModel;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using Rhino;
using Rhino.Commands;

namespace throughline.plugin.Mcp
{
    [McpServerToolType]
    public static class RhinoTools
    {
        [McpServerTool, Description("Return a friendly hello from the Rhino MCP server.")]
        public static string Hello(string name = "world") => $"Hello, {name} 👋 from Rhino!";

        [McpServerTool, Description("Summarize the active document (layers, objects).")]
        public static async Task<string> GetDocSummary()
        {
            // Safely hop to Rhino UI thread when touching doc state.
            var tcs = new TaskCompletionSource<string>();
            RhinoApp.InvokeOnUiThread((Action)(() =>
            {
                try
                {
                    var doc = RhinoDoc.ActiveDoc;
                    if (doc is null)
                    {
                        tcs.SetResult("No active Rhino document.");
                        return;
                    }

                    var layerCount = doc.Layers.Count;
                    var objectCount = doc.Objects.Count;
                    var name = string.IsNullOrEmpty(doc.Name) ? "(untitled)" : doc.Name;

                    tcs.SetResult($"Doc: {name}\nLayers: {layerCount}\nObjects: {objectCount}");
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }));

            return await tcs.Task.ConfigureAwait(false);
        }
    }
}
