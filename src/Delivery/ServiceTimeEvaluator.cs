using Google.OrTools.ConstraintSolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Delivery
{
    public class ServiceTimeEvaluator: NodeEvaluator2
    {
        private readonly NodeEvaluator2 _distanceEvaluator;
        private readonly double _speed;

        public ServiceTimeEvaluator(NodeEvaluator2 distanceEvaluator, double speed)
        {
            _distanceEvaluator = distanceEvaluator;
            _speed = speed;
        }

        public override long Run(int i, int j)
        {
            return (long)(_distanceEvaluator.Run(i, j) / _speed);
        }
    }
}
