using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Robocode;
using Romanchuk.Helpers;

namespace Romanchuk.MoveStrategy
{

    public class SpiralMoveStrategy : IMoveStrategy
    {
        private readonly Robot myRobot;
       
        public PointF DestinationPoint = new PointF(0,0);

        public bool UnsafeMovement
        {
            get
            {
                return true;
            }
        }
        
        public SpiralMoveStrategy(Robot robot)
        {
            myRobot = robot;
            const int zonesInLine = 3;
            var zoneWidth = myRobot.BattleFieldWidth / zonesInLine;
            var zoneHeight = myRobot.BattleFieldHeight / zonesInLine;
        }

        public PointF GetDestination(IEnumerable<Enemy> enemies, Enemy currentTarget, bool forceChangeDirection)
        {
            var center = new PointF((float)myRobot.BattleFieldWidth / 2, (float)myRobot.BattleFieldHeight / 2);
            if (currentTarget == null)
            {
                DestinationPoint = center;
                return DestinationPoint;
            }
            var currentPoint = new PointF((float)myRobot.X, (float)myRobot.Y);
            var targetPoint = new PointF((float)currentTarget.X, (float)currentTarget.Y);
            var distance = MathHelpers.CalculateDistance(targetPoint, currentPoint);
            if (DestinationPoint == center || Math.Abs(DestinationPoint.X - myRobot.X) <= 80 && Math.Abs(DestinationPoint.Y - myRobot.Y) <= 80)
            {
                var points = GetSpiralPoints(targetPoint, 7, 0, (float)distance);
                var p = points.FirstOrDefault(pp => Math.Abs(pp.X - myRobot.X) < myRobot.Width && Math.Abs(pp.X - myRobot.X) < myRobot.Width);
                DestinationPoint = p.IsEmpty ? points[0] : p;
            }
            return DestinationPoint;
        }

        // Return points that define a spiral.
        private List<PointF> GetSpiralPoints(
            PointF center, float A,
            float angle_offset, float max_r)
        {
            // Get the points.
            List<PointF> points = new List<PointF>();
            const float dtheta = (float)(5 * Math.PI / 180);    // Five degrees.
            for (float theta = 0; ; theta += dtheta)
            {
                // Calculate r.
                float r = A * theta;

                // Convert to Cartesian coordinates.
                float x, y;
                PolarToCartesian(r, theta + angle_offset, out x, out y);

                // Center.
                x += center.X;
                y += center.Y;

                // Create the point.
                points.Add(new PointF((float)x, (float)y));

                // If we have gone far enough, stop.
                if (r > max_r) break;
            }
            return points;
        }


        // Convert polar coordinates into Cartesian coordinates.
        private void PolarToCartesian(float r, float theta,
            out float x, out float y)
        {
            x = (float)(r * Math.Cos(theta));
            y = (float)(r * Math.Sin(theta));
        }

    }
}
