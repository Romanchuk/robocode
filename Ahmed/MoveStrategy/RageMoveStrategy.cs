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
            if (currentTarget.Instance.Distance < 300)
            {
                DestinationPoint = new PointF((float)currentTarget.X, (float)currentTarget.Y);
            }
            else
            {
                var futureX = (float)currentTarget.GetFutureX(2);
                var futureY = (float)currentTarget.GetFutureY(2);
                DestinationPoint = new PointF(
                    (float)(futureX <= 0 || futureX >= _myRobot.BattleFieldWidth ? currentTarget.X : futureX),
                    (float)(futureY <= 0 || futureY >= _myRobot.BattleFieldHeight ? currentTarget.X : futureX)
                );
            }
            return DestinationPoint;
        }

        public void Move(IEnumerable<Enemy> enemies, Enemy currentTarget, bool beeingHit)
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
