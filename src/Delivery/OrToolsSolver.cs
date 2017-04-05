using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoJSON.Net.Feature;
using Google.OrTools.ConstraintSolver;
using GeoJSON.Net.Geometry;

namespace Delivery
{
    public class OrToolsSolver
    {
        public FeatureCollection Solve(FeatureCollection input)
        {
            if (!input.Features.TrueForAll(x => x.Geometry is Point))
            {
                throw new ArgumentException("All Feature Geometries must be of type Point");
            }

            int numberOfNodes = input.Features.Count;
            var starts = new List<int>();
            var ends = new List<int>();
            int numberOfDrivers = 0;
            List<long> capacities = new List<long>();
            foreach (var driverNode in input.Features.Where(x => x.Properties.ContainsKey("driverId")))
            {
                numberOfDrivers += 1;
                capacities.Add(3);
                starts.Add(input.Features.IndexOf(driverNode));
                ends.Add(input.Features.IndexOf(driverNode));
            }

            var routing = new RoutingModel(numberOfNodes, numberOfDrivers, starts.ToArray(), ends.ToArray());

            AddPickupAndDropoff(input, routing);
            var evaluator = SetCost(input, routing);
            //SetCapacity(input, routing, capacities.ToArray());
            AddTimeDimension(input, routing, evaluator);

            var parameters = RoutingModel.DefaultSearchParameters();
            parameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;

            // The problem is solved here
            var solution = routing.SolveWithParameters(parameters);

            var output = new FeatureCollection(input.Features);
            for (int routeNumber = 0; routeNumber < numberOfDrivers; routeNumber++)
            {
                var positions = new List<IPosition>();
                string strokeColor = "";
                for (long node = routing.Start(routeNumber); !routing.IsEnd(node); node = solution.Value(routing.NextVar(node)))
                {
                    var feature = input.Features[(int)node];
                    var point = (Point)feature.Geometry;
                    positions.Add(point.Coordinates);
                    if(feature.Properties.ContainsKey("marker-color"))
                    {
                        strokeColor = (string)feature.Properties["marker-color"];
                    }
                }
                if (positions.Count >= 2)
                {
                    var properties = new Dictionary<string, object>
                    {
                        ["stroke"] = strokeColor
                    };
                    output.Features.Add(new Feature(new LineString(positions), properties));
                }
            }

            return output;
        }

        private static void AddTimeDimension(FeatureCollection input, RoutingModel routing, NodeEvaluator2 evaluator)
        {
            var speed = 10 * 1000 / 3600; // 10 km/h
            routing.AddDimension(new ServiceTimeEvaluator(evaluator, speed), 24 * 3600, 24 * 3600, true, "Time");
            var timeDimension = routing.GetDimensionOrDie("Time");
            var timeWindow = 3600 * 3;
            for (int i = 0; i < input.Features.Count; i++)
            {
                var feature = input.Features[i];
                if (feature.Properties.ContainsKey("start"))
                {
                    int start = Convert.ToInt32(feature.Properties["start"]),
                        end = start + timeWindow;
                    timeDimension.CumulVar(routing.NodeToIndex(i)).SetRange(start, end);
                }
            }
        }

        private static NodeEvaluator2 SetCost(FeatureCollection collection, RoutingModel routing)
        {
            var coordinates = collection
                            .Features
                            .Select(x => x.Geometry)
                            .Cast<Point>()
                            .Select(x => x.Coordinates)
                            .Cast<GeographicPosition>()
                            .ToArray();

            var nodeEvaluator = new HaversineDistanceEvaluator(coordinates);
            routing.SetCost(nodeEvaluator);

            return nodeEvaluator;
        }

        private static void SetCapacity(FeatureCollection collection, RoutingModel routing, long[] capacities)
        {
            routing.AddDimensionWithVehicleCapacity(new DemandCallback(collection.Features), 0, capacities, true, "capacity");
        }

        private static void AddPickupAndDropoff(FeatureCollection run, RoutingModel routing)
        {
            const int pickup = 0, dropoff = 1;
            var groups = run
                            .Features
                            .Where(x => x.Properties.ContainsKey("orderId"))
                            .GroupBy(x => x.Properties["orderId"]);

            foreach (var group in groups)
            {
                var pickupStop = group.Single(x => Convert.ToInt32(x.Properties["stopKind"]) == pickup);
                var dropoffStop = group.Single(x => Convert.ToInt32(x.Properties["stopKind"]) == dropoff);

                int pickupNode = run.Features.IndexOf(pickupStop),
                    dropoffNode = run.Features.IndexOf(dropoffStop);
                
                // Set route precedence
                routing.AddPickupAndDelivery(pickupNode, dropoffNode);
                // Link same vehicle to both nodes
                var constraint = routing
                    .solver()
                    .MakeEquality(
                        routing.VehicleVar(routing.NodeToIndex(pickupNode)),
                        routing.VehicleVar(routing.NodeToIndex(dropoffNode)));

                routing.solver().Add(constraint);
                
            }
        }

        class DemandCallback: NodeEvaluator2
        {
            IList<Feature> _nodes;
            public DemandCallback(IList<Feature> nodes)
            {
                _nodes = nodes;
            }

            public override long Run(int i, int j)
            {
                var node = _nodes[i];
                bool isPickup = node.Properties.ContainsKey("stopKind")
                    && Convert.ToInt32(node.Properties["stopKind"]) == 0;

                bool isDropoff = node.Properties.ContainsKey("stopKind")
                    && Convert.ToInt32(node.Properties["stopKind"]) == 1;

                return isPickup ? 1L : 0L;
            }
        }
    }
}
