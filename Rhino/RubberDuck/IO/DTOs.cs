using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubberDuck.IO
{
    public class MepDto
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("area")]
        public double Area { get; set; }

        [JsonProperty("height")]
        public double Height { get; set; }

        [JsonProperty("geometry")]
        public DuckGeometry Geometry { get; set; }

    }

    public class DuckGeometry
    {
        [JsonProperty("points")]
        public List<DuckPoint> Points { get; set; }

        public DuckGeometry()
        {
            Points = new List<DuckPoint>();
        }

        public DuckGeometry(List<DuckPoint> points)
        {
            Points = points;
        }
    }

    public class DuckPoint
    {
        [JsonProperty("x")]
        public double X { get; set; }
        [JsonProperty("y")]
        public double Y { get; set; }
        [JsonProperty("z")]
        public double Z { get; set; }

        public DuckPoint()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public DuckPoint(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
