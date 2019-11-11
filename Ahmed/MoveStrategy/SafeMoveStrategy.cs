using System;
using System.Drawing;
using System.Linq;
using Robocode;

namespace Romanchuk.MoveStrategy
{

    class Zone
    {
        public Zone(PointF leftBottom, PointF rightTop)
        {
            LeftBottom = leftBottom;
            RightTop = rightTop;
            ThreatLevel = 0;
        }

        public PointF LeftBottom;
        public PointF RightTop;

        public double ThreatLevel;

        public PointF GetCenterPoint()
        {
            return new PointF(RightTop.X - (RightTop.X - LeftBottom.X)/2, RightTop.Y - (RightTop.Y - LeftBottom.Y)/2);
        }
    }

    public class SafeMoveStrategy : IMoveStrategy
    {
        private readonly Zone[] _zones = new Zone[9];

        public SafeMoveStrategy(double battleFieldHeight, double battleFieldWidth)
        {
            const int zonesInLine = 3;
            var zoneWidth = battleFieldWidth / zonesInLine;
            var zoneHeight = battleFieldHeight / zonesInLine;

            var baseX = 0d;
            var baseY = 0d;
            // 0,0 - Left, Bottom
            for (var i = 0; i < 9; i++)
            {                
                _zones[i] = new Zone(
                    new PointF((float)(baseX), (int)baseY),
                    new PointF((float)(baseX + zoneWidth), (float)(baseY + zoneHeight))
                );
                if ((i + 1) % 3 == 0)
                {
                    baseX = 0;
                    baseY += zoneHeight;
                }
                else
                {
                    baseX += zoneWidth;
                }
            }
        }

        public PointF SetDestination(Enemy[] enemies, Robot myRobot)
        {
            for (var i = 0; i < _zones.Length; i++)
            {
                _zones[i].ThreatLevel = 0;
            }

            foreach (var e in enemies)
            {
                var countZone = CoordsInZone(e.X, e.Y);
                countZone.ThreatLevel += 1;                
            }
            // Zone myRobotZone = CoordsInZone(myRobot.X, myRobot.Y);


            var zones = _zones.Select(e => new
                           {
                               e,
                               distance = Math.Sqrt(Math.Pow((myRobot.X - e.GetCenterPoint().X), 2) + Math.Pow((myRobot.Y - e.GetCenterPoint().Y), 2))
                           });

              var minDistZone = zones
                    .OrderBy(z => z.e.ThreatLevel)
                    .ThenBy(z => z.distance)                 
                    .First();
            return minDistZone.e.GetCenterPoint();
        }

        private Zone CoordsInZone(double X, double Y)
        {
            return _zones.FirstOrDefault(z => z.LeftBottom.X <= X
                                                          && z.RightTop.X >= X
                                                          && z.LeftBottom.Y <= Y
                                                          && z.RightTop.Y >= Y);
        }

    }
}
