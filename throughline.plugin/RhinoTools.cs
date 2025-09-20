// RhinoTools.cs
using System;
using System.ComponentModel;
using ModelContextProtocol.Server;
using Rhino;
using Rhino.Geometry;

namespace throughline.plugin
{
    /// <summary>
    /// Group your Rhino-facing tools here. Keep methods static.
    /// </summary>
    [McpServerToolType, Description("Tools for inspecting and editing the active Rhino document. Use cautiously; many actions must run on the UI thread.")]
    public static class RhinoTools
    {
        [McpServerTool(Name = "get_doc_summary", Title = "Get Document Summary")]
        [Description("Returns basic info about the active Rhino document (name, unit system, object count).")]
        public static object GetDocumentSummary()
        {
            // READ-ONLY is generally safe from any thread, but invoking on UI thread is safest overall.
            string name = RhinoDoc.ActiveDoc?.Name ?? "(untitled)";
            var units = RhinoDoc.ActiveDoc?.ModelUnitSystem.ToString() ?? "Unknown";
            int count = RhinoDoc.ActiveDoc?.Objects?.Count ?? 0;

            return new
            {
                name,
                units,
                objectCount = count
            };
        }

        // AddPoint: run synchronously on Rhino's UI thread and then return the Guid.
        [McpServerTool(Name = "add_point", Title = "Add Point")]
        [Description("Adds a point object at the given coordinates. Returns the new object ID.")]

        public static Guid AddPoint(double x, double y, double z = 0.0)
        {
            Guid id = Guid.Empty;

            RhinoApp.InvokeAndWait(() =>
            {
                var doc = RhinoDoc.ActiveDoc;
                if (doc == null) return;

                id = doc.Objects.AddPoint(new Point3d(x, y, z));
                if (id != Guid.Empty) doc.Views.Redraw();
            });

            return id;
        }


        // AddLayer: also prefer InvokeAndWait to ensure we have the index before returning.
        [McpServerTool(Name = "add_layer", Title = "Add Layer")]
        [Description("Creates a new layer if it doesn't exist and returns its index.")]

        public static int AddLayer(string layerName)
        {
            int layerIndex = -1;

            RhinoApp.InvokeAndWait(() =>
            {
                var doc = RhinoDoc.ActiveDoc;
                if (doc == null) return;

                var existing = doc.Layers.FindName(layerName);
                if (existing != null)
                {
                    layerIndex = existing.Index;
                    return;
                }

                var newLayer = new Rhino.DocObjects.Layer { Name = layerName };
                layerIndex = doc.Layers.Add(newLayer);
                doc.Views.Redraw();
            });

            return layerIndex;
        }


        // Add more placeholders as needed...
        // [McpServerTool] public static object GetLayerTable() => ...
        // [McpServerTool] public static Guid AddLine(double x1, double y1, double z1, double x2, double y2, double z2) => ...
    }
}
