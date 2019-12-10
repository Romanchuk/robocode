using System;
using Robocode;
using Robocode.Util;

namespace Romanchuk
{
    public class Enemy
    {
        private readonly Robot _myRobot;

        public Enemy(Robot robot)
        {
            _myRobot = robot;
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

            double absBearingDeg = (_myRobot.Heading + e.Bearing);
            if (absBearingDeg < 0) absBearingDeg += 360;

            // yes, you use the _sine_ to get the X value because 0 deg is North
            X = _myRobot.X + Math.Sin(Utils.ToRadians(absBearingDeg)) * e.Distance;

            // yes, you use the _cosine_ to get the Y value because 0 deg is North
            Y = _myRobot.Y + Math.Cos(Utils.ToRadians(absBearingDeg)) * e.Distance;
        }

        public double GetFutureX(long momentOfTime)
        {
            if (Instance == null)
            {
                throw new Exception("There is no target instance");
            }
            var futureX = X + Math.Sin(Instance.HeadingRadians) * Instance.Velocity * momentOfTime;
            futureX += Instance.Velocity >= 6 ? Math.Cos(Instance.HeadingRadians) * Instance.Velocity/2 : 0;

            var xo = futureX - X;
            if (futureX < _myRobot.Width / 2)
            {
                futureX = _myRobot.Width / 2;
            }
            var maxX = _myRobot.BattleFieldWidth - _myRobot.Width / 2;
            if (futureX > maxX)
            {
                futureX = maxX;
            }

            return futureX;
        }

        public double GetFutureY(long momentOfTime)
        {
            if (Instance == null)
            {
                throw new Exception("There is no target instanЯe");
            }
            var futureY = Y + Math.Cos(Instance.HeadingRadians) * Instance.Velocity * momentOfTime ;
            futureY += Instance.Velocity >= 6 ? Math.Cos(Instance.HeadingRadians) * Instance.Velocity / 2 : 0;
            var yo = futureY - Y;
            if (futureY < _myRobot.Width/2)
            {
                futureY = _myRobot.Width/2;
            }

            var maxY = _myRobot.BattleFieldHeight - _myRobot.Width / 2;
            if (futureY > maxY)
            {
                futureY = maxY;
            }

            return futureY;
        }

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
