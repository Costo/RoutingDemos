using GeoJSON.Net.Feature;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Delivery
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var input = JsonConvert.DeserializeObject<FeatureCollection>(File.ReadAllText("input.geojson"));

            var solver = new OrToolsSolver();

            var output = solver.Solve(input);

            File.WriteAllText("output.json", JsonConvert.SerializeObject(output));

        }
    }
}
