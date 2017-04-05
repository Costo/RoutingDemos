using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Tsp
{
    public class GoogleDirectionsTsp
    {
        private const string DirectionsApiUrl = "https://maps.googleapis.com/maps/api/directions/";
        private readonly string _apiKey;
        public GoogleDirectionsTsp(string apiKey)
        {
            _apiKey = apiKey;
        }

        public string[] Solve(string[] nodes)
        {
            var http = new HttpClient()
            {
                BaseAddress = new Uri(DirectionsApiUrl, UriKind.Absolute),
            };

            var qs = QueryString.Create(new Dictionary<string, string>
            {
                ["origin"] = nodes[0],
                ["destination"] = nodes[0],
                ["waypoints"] = string.Join("|", new[] { "optimize:true" }.Concat(nodes.Skip(1).Take(nodes.Length - 1))),
                ["key"] = _apiKey,
            });

            var url = "json" + qs;

            Console.WriteLine($"Request Url: {url}");
            Console.WriteLine();

            var response = http.GetAsync(url).Result;
            var json = Newtonsoft.Json.Linq.JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var order = json["routes"][0]["waypoint_order"].ToObject<int[]>();

            var result = new List<string>();
            result.Add(nodes[0]);
            foreach (var o in order)
            {
                result.Add(nodes.ElementAt(o + 1));
            }
            result.Add(nodes[nodes.Length - 1]);

            return result.ToArray();
        }
    }
}
