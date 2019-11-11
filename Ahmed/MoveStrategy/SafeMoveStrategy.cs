using System;
using System.Drawing;
using System.Linq;
using Robocode;

namespace Romanchuk.MoveStrategy
{

    struct Zone
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
                if ((i + 1) % 3 == 0)
                {
                    baseX = 0;
                    baseY += zoneHeight;
                }
                else
                {
                    baseX += zoneWidth;
                }
                _zones[i] = new Zone(
                    new PointF((float)(baseX), (int)baseY),
                    new PointF((float)(baseX + zoneWidth), (float)(baseY + zoneHeight))
                );
            }
        }

        public Point SetDestination(Enemy[] enemies, Robot myRobot)
        {
            for (var i = 0; i < _zones.Length; i++)
            {
                var z = _zones[i].ThreatLevel = 0;
            }

            foreach (var e in enemies)
            {
                var countZone =_zones.FirstOrDefault(z => z.LeftBottom.X <= e.X 
                                                          && z.RightTop.X >= e.X 
                                                          && z.LeftBottom.Y <= e.Y 
                                                          && z.RightTop.Y >= e.Y);
                if (Array.IndexOf(_zones, countZone) != -1)
                {
                    countZone.ThreatLevel += 1;
                }
            }
            var cosestAndSafiestZone = 


        }

    }
}
