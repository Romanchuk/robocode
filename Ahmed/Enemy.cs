using System;
using Robocode;
using Robocode.Util;

namespace Romanchuk
{
    public class Enemy
    {
        public readonly Robot myRobot;

        public Enemy(Robot robot)
        {
            myRobot = robot;
        }


        public ScannedRobotEvent Instance;

        public string Name => Instance?.Name;

        public double PreviousTurnEnergy = -1d;

        public bool LostEnergy
        {
            get
            {
                if (PreviousTurnEnergy.Equals(-1d))
                {
                    return false;
                }
                var energyDiff = PreviousTurnEnergy - Instance.Energy;
                return energyDiff > 0;
            }
        }

        public bool JustShooted
        {
            get
            {
                if (PreviousTurnEnergy.Equals(-1d))
                {
                    return false;
                }
                var energyDiff = PreviousTurnEnergy - Instance.Energy;
                return energyDiff > 0d && energyDiff <= 3d;
            }
        }

        public double X { get; set; }
        public double Y { get; set; }

        public void Update(ScannedRobotEvent e)
        {
            if (Instance != null && e.Time > Instance.Time)
            {
                PreviousTurnEnergy = Instance.Energy;
            }
            Instance = e;

            double absBearingDeg = (myRobot.Heading + e.Bearing);
            if (absBearingDeg < 0) absBearingDeg += 360;

            // yes, you use the _sine_ to get the X value because 0 deg is North
            X = myRobot.X + Math.Sin(Utils.ToRadians(absBearingDeg)) * e.Distance;

            // yes, you use the _cosine_ to get the Y value because 0 deg is North
            Y = myRobot.Y + Math.Cos(Utils.ToRadians(absBearingDeg)) * e.Distance;
        }

        public double GetFutureX(long momentOfTime)
        {
            if (Instance == null)
            {
                throw new Exception("There is no target instance");
            }
            var futureX = X + Math.Sin(Instance.HeadingRadians) * Instance.Velocity * momentOfTime;

            var xo = futureX - X;
            var bonusForDistance = (Instance.Distance > 500 && Instance.Velocity > 4 ? Instance.Velocity : 0);
            futureX += (xo > 0 ? 1 : -1) * bonusForDistance;
            if (futureX < 0) { futureX = 0; }
            if (futureX > myRobot.BattleFieldWidth) { futureX = myRobot.BattleFieldWidth; }

            return futureX;
        }

        public double GetFutureY(long momentOfTime)
        {
            if (Instance == null)
            {
                throw new Exception("There is no target instance");
            }
            var futureY = Y + Math.Cos(Instance.HeadingRadians) * Instance.Velocity * momentOfTime;
            var yo = futureY - Y;
            var bonusForDistance = (Instance.Distance > 500 && Instance.Velocity > 4 ? Instance.Velocity : 0);
            futureY += (yo > 0 ? 1 : -1) * bonusForDistance;
            if (futureY < 0) { futureY = 0; }
            if (futureY > myRobot.BattleFieldHeight) { futureY = myRobot.BattleFieldHeight; }

            return futureY;
        }
        // TODO: Прицеливание когда танки на одной линии
        public double GetFutureT(Robot myRobot, double bulletVelocity)
        {

            // enemy velocity
            double velocity = Instance.Velocity;

            // temp variables
            double x_diff = X - myRobot.X;
            double y_diff = Y - myRobot.Y;

            // angles of enemy's heading
            double sin = Math.Sin(Instance.HeadingRadians);
            double cos = Math.Cos(Instance.HeadingRadians);

           

            double xy = (x_diff * sin + y_diff * cos);

            // calculated time
            double T = ((velocity * xy) + Math.Sqrt(Math.Pow(velocity, 2) * Math.Pow(xy,2) + (Math.Pow(x_diff, 2) + Math.Pow(y_diff, 2) * (Math.Pow(bulletVelocity, 2) + Math.Pow(velocity, 2))))) / (Math.Pow(bulletVelocity, 2) - Math.Pow(velocity, 2));
            return T;

        }

    }
}
