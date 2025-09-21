using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubberDuck
{
    public static class PathController
    {
        public static string AppData => Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData, 
            Environment.SpecialFolderOption.Create);

        public static string RubberDuckFolder => System.IO.Path.Combine(AppData, "RubberDuck");

        public static string ExportsFolder => System.IO.Path.Combine(RubberDuckFolder, "Exports");

        public static string ExportFilePath => System.IO.Path.Combine(ExportsFolder, "export.json");
    }
}
