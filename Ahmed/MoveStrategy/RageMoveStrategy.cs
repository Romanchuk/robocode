using System;
using System.Collections.Generic;
using System.Drawing;
using Robocode;
using Romanchuk.Helpers;

namespace Romanchuk.MoveStrategy
{
    public class RageMoveStrategy : IMoveStrategy
    {
        private readonly AdvancedRobot _myRobot;
       
        public PointF DestinationPoint = new PointF(0,0);

        public bool UnsafeMovement => true;

        public RageMoveStrategy(AdvancedRobot robot)
        {
            _myRobot = robot;
        }

        public PointF GetDestination(IEnumerable<Enemy> enemies, Enemy currentTarget, bool forceChangeDirection)
        {
            if (currentTarget.Instance.Distance < 200)
            {
                DestinationPoint = new PointF((float)currentTarget.X, (float)currentTarget.Y);
            }
            else
            {
                DestinationPoint = new PointF((float)currentTarget.GetFutureX(1), (float)currentTarget.GetFutureY(1));
            }
            return MathHelpers.CorrectPointOnBorders(DestinationPoint, _myRobot.BattleFieldWidth, _myRobot.BattleFieldHeight, (float)(_myRobot.Width * 2));
        }

        public void Move(IEnumerable<Enemy> enemies, Enemy currentTarget, bool forceChangeDirection)
        {
            var dest = GetDestination(enemies, currentTarget, false);
            var angleToTurn = MathHelpers.TurnRobotToPoint(_myRobot, dest);

            _myRobot.SetTurnRight(angleToTurn);

            var distance = Config.MaxDistancePerTurn;
            if (Math.Abs(angleToTurn) > 40)
            {
                distance = Config.MaxDistancePerTurn / 2;
            }
            else if (Math.Abs(angleToTurn) > 90)
            {
                distance = Config.MaxDistancePerTurn / 4;
            }

            _myRobot.SetAhead(distance);
        }

    }
}
