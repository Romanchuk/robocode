using System;
using Robocode;
using Robocode.Util;

namespace Romanchuk
{
    public class Target
    {
        public readonly Robot myRobot;

        public Target(Robot robot)
        {
            myRobot = robot;
        }


        public ScannedRobotEvent Instance = null;

        public string Name => this.Instance?.Name;

        public bool None => this.Instance == null;

        public double X { get; set; }
        public double Y { get; set; }

        public void Set(ScannedRobotEvent e)
        {
            this.Instance = e;

            double absBearingDeg = (myRobot.Heading + e.Bearing);
            if (absBearingDeg < 0) absBearingDeg += 360;

            // yes, you use the _sine_ to get the X value because 0 deg is North
            X = myRobot.X + Math.Sin(Utils.ToRadians(absBearingDeg)) * e.Distance;

            // yes, you use the _cosine_ to get the Y value because 0 deg is North
            Y = myRobot.Y + Math.Cos(Utils.ToRadians(absBearingDeg)) * e.Distance;
        }


        public void Reset()
        {
            Instance = null;
        }

        public double GetFutureX(long momentOfTime)
        {
            if (this.Instance == null)
            {
                throw new Exception("There is no target instance");
            }
            return X + Math.Sin(this.Instance.HeadingRadians) * this.Instance.Velocity * momentOfTime;
        }

        public double GetFutureY(long momentOfTime)
        {
            if (this.Instance == null)
            {
                throw new Exception("There is no target instance");
            }
            return Y + Math.Cos(this.Instance.HeadingRadians) * this.Instance.Velocity * momentOfTime;
        }

        public double GetFutureT(Robot myRobot, double bulletVelocity)
        {

            // enemy velocity
            double velocity = this.Instance.Velocity;

            // temp variables
            double x_diff = X - myRobot.X;
            double y_diff = Y - myRobot.Y;

            // angles of enemy's heading
            double sin = Math.Sin(this.Instance.HeadingRadians);
            double cos = Math.Cos(this.Instance.HeadingRadians);

           

            double xy = (x_diff * sin + y_diff * cos);

            // calculated time
            double T = ((velocity * xy) + Math.Sqrt(sqr(velocity) * sqr(xy) + (sqr(x_diff) + sqr(y_diff)) * (sqr(bulletVelocity) + sqr(velocity)))) / (sqr(bulletVelocity) - sqr(velocity));
            return T;

        }

        private double sqr(double p)
        {
            return p * p;
        }


    }
}
