using System;
using System.Collections.Generic;
using System.Linq; // for .Select
using Newtonsoft.Json;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using RubberDuck.IO;

namespace RubberDuck
{
    public class DuckParse : Command
    {
        private readonly List<string> _layerList = new List<string>()
        {
            "MEP",
            "Serviced",
            "Not Serviced"
        };

        public DuckParse()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a reference in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static DuckParse Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "DuckParse";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            RhinoApp.WriteLine("Just being a duck");

            // Determine which layers to process
            var layersToProcess = _layerList.Count > 0
                ? _layerList
                : doc.Layers.Select(l => l.FullPath).ToList();

            List<MepDto> dtos = new List<MepDto>();

            foreach (var layerName in layersToProcess)
            {

                var layer = doc.Layers.FindName(layerName);
                if (layer == null)
                {
                    RhinoApp.WriteLine($"Layer not found: \"{layerName}\"");
                    continue;
                }

                RhinoObject[] objsOnLayer = doc.Objects.FindByLayer(layer);
                if (objsOnLayer == null || objsOnLayer.Length == 0)
                {
                    RhinoApp.WriteLine($"No objects found on layer: \"{layerName}\"");
                    continue;
                }

                foreach (var obj in objsOnLayer)
                {
                    MepDto dto = new MepDto();

                    // Get ID
                    dto.Id = obj.Id.ToString();
                    dto.Name = obj.Name ?? "";

                    DuckGeomUtils.TryBuildDtoFromRhinoObject(doc, obj, out dto);

                    dtos.Add(dto);

                    // (Optional) If you later want to emit JSON:
                    // RhinoApp.WriteLine(JsonConvert.SerializeObject(dto, Formatting.None));
                }

            }

            string serialized = JsonConvert.SerializeObject(dtos, Formatting.Indented);

            if (!System.IO.Directory.Exists(PathController.ExportsFolder))
            {
                System.IO.Directory.CreateDirectory(PathController.ExportsFolder);
            }

            using (var writer = new System.IO.StreamWriter(PathController.ExportFilePath, false))
            {
                writer.Write(serialized);
            }

            return Result.Success;
        }
    }
}
