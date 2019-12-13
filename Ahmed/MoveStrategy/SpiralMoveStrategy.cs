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
        private readonly AdvancedRobot _myRobot;
        private readonly PointF _centerPoint;

        public PointF DestinationPoint;

        public bool UnsafeMovement => true;

        private const float MaxRadius = 280;
        private float CurrentMoveRadius = MaxRadius;
        private bool ShouldChangeDirection;

        private long _lastForceDirectionChange;

        public SpiralMoveStrategy(AdvancedRobot robot)
        {
            _myRobot = robot;
            DestinationPoint = _centerPoint =
                new PointF((float) _myRobot.BattleFieldWidth / 2, (float) _myRobot.BattleFieldHeight / 2);
            if (_myRobot.Others == 1)
            {
                CurrentMoveRadius = 220;
            }
        }

        public PointF GetDestination(IEnumerable<Enemy> enemies, Enemy currentTarget, bool forceChangeDirection)
        {
            if (currentTarget == null)
            {
                // DestinationPoint = _centerPoint;
                return DestinationPoint;
            }

            ShouldChangeDirection = currentTarget.Instance.Distance > 100 && forceChangeDirection &&
                                (_lastForceDirectionChange + 8) < _myRobot.Time;

            var currentPoint = new PointF((float)_myRobot.X, (float)_myRobot.Y);
            var targetPoint = new PointF((float)currentTarget.X, (float)currentTarget.Y);

            float allowedDiff = 80;

            var achievedPoint = AchievedPoint(DestinationPoint, currentPoint, allowedDiff);

            if (DestinationPoint == _centerPoint || achievedPoint || ShouldChangeDirection)
            {
                var points = GetRoundPoints(targetPoint);

                PointF p;
                if (ShouldChangeDirection)
                {
                    _lastForceDirectionChange = _myRobot.Time;
                    var pts = points
                        .Where(pp => !AchievedPoint(pp, currentPoint, allowedDiff * 2))
                        .ToArray();
                    var r = new Random();
                    var ind = r.Next(0, pts.Length - 1);
                    p = pts[ind];
                } else
                {
                    p = points
                        .FirstOrDefault(pp => !AchievedPoint(pp, currentPoint, allowedDiff));
                }

                var newPoint = p.IsEmpty ? points.First() : p;

                DestinationPoint = MathHelpers.CorrectPointOnBorders(newPoint, _myRobot.BattleFieldWidth,
                    _myRobot.BattleFieldHeight, allowedDiff);
            }
            return DestinationPoint;
        }

        private bool AchievedPoint(PointF destPoint, PointF curPoint, float allowedDiff)
        {
            return Math.Abs(destPoint.X - curPoint.X) <= allowedDiff &&
                   Math.Abs(destPoint.Y - curPoint.Y) <= allowedDiff;
        }

        public void Move(IEnumerable<Enemy> enemies, Enemy currentTarget, bool underAttack)
        {
            var dest = GetDestination(enemies, currentTarget, currentTarget?.JustShooted ?? false || underAttack);
            var angleToTurn = MathHelpers.TurnRobotToPoint(_myRobot, dest);

            double direction = 1;

            if (Math.Abs(angleToTurn) >= 120)
            {
                angleToTurn = 180 - angleToTurn;
                direction = -1;
            }
            var r = new Random();
            
            _myRobot.SetTurnRight(angleToTurn);
            _myRobot.SetAhead(
                (ShouldChangeDirection ?
                    r.Next((int)Config.MaxDistancePerTurn/2, (int)Config.MaxDistancePerTurn) :
                    Config.MaxDistancePerTurn)
                * direction);
        }

        private IEnumerable<PointF> GetRoundPoints(PointF target)
        {
            // Get the points.
            List<PointF> points = new List<PointF>();
            const float angleStep = 20;    // 20 degrees.
            for (float theta = 0; theta <= 360; theta += angleStep)
            {
                // Convert to Cartesian coordinates.
                float x, y;
                PolarToCartesian(CurrentMoveRadius, theta, out x, out y);

                // Center.
                x += target.X;
                y += target.Y;

                // Create the point.
                points.Add(new PointF((float)x, (float)y));
            }

            return points
                .OrderBy(b => Math.Abs(MathHelpers.TurnRobotToPoint(_myRobot, b)));
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
