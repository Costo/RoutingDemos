using Google.OrTools.ConstraintSolver;
using GoogleMapsServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tsp
{
    public class OrToolsTsp
    {
        private readonly GoogleMaps _maps;
        public OrToolsTsp(GoogleMaps maps)
        {
            _maps = maps;
        }
        public string[] Solve(string[] nodes)
        {

            var routing = new RoutingModel(nodes.Length, 1, 0);
            routing.SetCost(new HaversineDistanceEvaluator (nodes, _maps));
            var parameters = RoutingModel.DefaultSearchParameters();
            parameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;
            var solution = routing.SolveWithParameters(parameters);

            int routeNumber = 0;
            var results = new List<string>();
            for (long node = routing.Start(routeNumber); !routing.IsEnd(node); node = solution.Value(routing.NextVar(node)))
            {
                results.Add(nodes[routing.IndexToNode(node)]);
            }

            return results.ToArray();

        }

        public class HaversineDistanceEvaluator : NodeEvaluator2
        {
            readonly string[] _nodes;
            readonly GoogleMaps _maps;
            public HaversineDistanceEvaluator (string[] nodes, GoogleMaps maps)
            {
                _nodes = nodes;
                _maps = maps;
            }

            public override long Run(int i, int j)
            {
                var location1 = _maps.Geocode(_nodes[i]).Result.Results.First().Geometry.Location;
                var location2 = _maps.Geocode(_nodes[j]).Result.Results.First().Geometry.Location;

                return (long)new Haversine().Distance(location1, location2);
            }

            class Haversine
            {
                public double Distance(LatLng pos1, LatLng pos2)
                {
                    var d1 = pos1.Latitude * (Math.PI / 180.0);
                    var num1 = pos1.Longitude * (Math.PI / 180.0);
                    var d2 = pos2.Latitude * (Math.PI / 180.0);
                    var num2 = pos2.Longitude * (Math.PI / 180.0) - num1;
                    var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) +
                             Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

                    return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
                }
            }
        }
          
    }


}
