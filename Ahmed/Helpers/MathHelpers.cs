using System;
using Robocode.Util;

namespace Romanchuk.Helpers
{
    public static class MathHelpers
    {
        public static double CalculateBulletSpeed(double firePower)
        { 
            return 20 - firePower * 3;
        }

        public static long CalculateBulletHitTime(double targetDistance, double bulletSpeed)
        {
            return (long)(targetDistance / bulletSpeed);
        }

        public static double AbsoluteBearingDegrees(double x1, double y1, double x2, double y2)
        {
            double xo = x2 - x1;
            double yo = y2 - y1;
            double dist = CalculateDistance(x1, y1, x2, y2);
            double arcSin = Utils.ToDegrees(Math.Asin(xo / dist));
            double bearing = 0;

            if (xo > 0 && yo > 0)
            { // both pos: lower-Left
                bearing = arcSin;
            }
            else if (xo < 0 && yo > 0)
            { // x neg, y pos: lower-right
                bearing = 360 + arcSin; // arcsin is negative here, actually 360 - ang
            }
            else if (xo > 0 && yo < 0)
            { // x pos, y neg: upper-left
                bearing = 180 - arcSin;
            }
            else if (xo < 0 && yo < 0)
            { // both neg: upper-right
                bearing = 180 - arcSin; // arcsin is negative here, actually 180 + ang
            }

            return bearing;
        }


        public static double CalculateDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));

        }

        public static double CalculateDiagonal(double width, double height)
        {
            return Math.Sqrt(Math.Pow(width, 2) + Math.Pow(height, 2));

        }

        public static double TurnRightOptimalAngle(double heading, double absBearing)
        {
            var clockwiseDiff = absBearing - heading;
            var сounterclockwiseDiff = heading + (360 - absBearing);
            double angleToTurn;
            if (Math.Abs(clockwiseDiff) < Math.Abs(сounterclockwiseDiff))
            {
                angleToTurn = clockwiseDiff;
            }
            else
            {
                angleToTurn = -сounterclockwiseDiff;
            }
            return angleToTurn;
        }
        
    }
}
