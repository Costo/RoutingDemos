using GeoJSON.Net.Geometry;
using Google.OrTools.ConstraintSolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Delivery
{
    public class HaversineDistanceEvaluator : NodeEvaluator2
    {
        private readonly IList<GeographicPosition> _nodes;
        private readonly Haversine _haversine;
        public HaversineDistanceEvaluator(IList<GeographicPosition> nodes)
        {
            _nodes = nodes;
            _haversine = new Haversine();
        }

        public override long Run(int i, int j)
        {
            return (long)_haversine.Distance(_nodes[i], _nodes[j]);
        }

        class Haversine
        {
            public double Distance(GeographicPosition pos1, GeographicPosition pos2)
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
