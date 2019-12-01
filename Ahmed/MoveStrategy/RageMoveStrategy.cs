using System.Collections.Generic;
using System.Drawing;
using Robocode;
using Romanchuk.Helpers;

namespace Romanchuk.MoveStrategy
{

    public class RageMoveStrategy : IMoveStrategy
    {
        private readonly AdvancedRobot myRobot;
       
        public PointF DestinationPoint = new PointF(0,0);

        public bool UnsafeMovement => true;

        public RageMoveStrategy(AdvancedRobot robot)
        {
            myRobot = robot;
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
            return MathHelpers.CorrectPointOnBorders(DestinationPoint, myRobot.BattleFieldWidth, myRobot.BattleFieldHeight, (float)(myRobot.Width * 2));
        }

        public void Move(IEnumerable<Enemy> enemies, Enemy currentTarget, bool forceChangeDirection)
        {
            var dest = GetDestination(enemies, currentTarget, false);
            var angleToTurn = MathHelpers.TurnRobotToPoint(myRobot, dest);

            myRobot.SetTurnRight(angleToTurn);
            if (angleToTurn < 180)
            {
                myRobot.SetAhead(Rules.MAX_VELOCITY);
            }
            else
            {
                myRobot.SetAhead(Rules.MAX_VELOCITY / 3);
            }
        }

    }
}
