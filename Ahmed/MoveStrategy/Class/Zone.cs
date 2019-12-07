using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Romanchuk.Helpers;

namespace Romanchuk.MoveStrategy.Class
{
    public class Zone
    {
        const double BASE_THREAT = 0.25;
        public Zone(PointF leftBottom, PointF rightTop)
        {
            LeftBottom = leftBottom;
            RightTop = rightTop;
        }

        public PointF LeftBottom;
        public PointF RightTop;

        public Zone[] AdjacentZones { get; set; }
        public List<Enemy> EnemiesInZone { get; set; } = new List<Enemy>();

        public double ThreatIndex
        {
            get
            {
                double enemiesThreat = 0;
                foreach (var e in EnemiesInZone)
                {
                    if (e.Instance.Energy < 18 && EnemiesInZone.Count == 1)
                    {
                        enemiesThreat += -(BASE_THREAT * 1.5);
                    }
                    else if (e.Instance.Energy < 20 && EnemiesInZone.Count <= 2)
                    {
                        enemiesThreat += -BASE_THREAT / 2;
                    }
                    else if (e.Instance.Energy < 30)
                    {
                        enemiesThreat += BASE_THREAT / 2;
                    }
                    else if (e.Instance.Energy < 40)
                    {
                        enemiesThreat += BASE_THREAT;
                    }
                    else
                    {
                        enemiesThreat += BASE_THREAT * 2;
                    }
                }

                double adjacentZonesThreat = AdjacentZones.Select(z => (double)z.EnemiesInZone.Count)
                    .Aggregate((c, r) => c * 0.15);

                return BaseThreatIndex + enemiesThreat + adjacentZonesThreat;
            }
        }

        public PointF GetCenterPoint()
        {
            return new PointF(RightTop.X - (RightTop.X - LeftBottom.X) / 2, RightTop.Y - (RightTop.Y - LeftBottom.Y) / 2);
        }

        public PointF GetRandomPoint()
        {
            var rand = new Random();
            return new PointF(rand.Next((int)LeftBottom.X, (int)RightTop.X), rand.Next((int)LeftBottom.Y, (int)RightTop.Y));
        }

        public PointF GetPointExcept(PointF currentPos, double minDist)
        {
            var lbDist = MathHelpers.CalculateDistance(currentPos, LeftBottom);
            var rtDist = MathHelpers.CalculateDistance(currentPos, RightTop);
            var rand = new Random();
            if (lbDist > rtDist)
            {
                var newX = (int)(currentPos.X - minDist);
                var newY = (int)(currentPos.Y - minDist);
                return new PointF(
                    rand.Next((int)LeftBottom.X, (int)(newX < LeftBottom.X ? LeftBottom.X : newX)),
                    rand.Next((int)LeftBottom.Y, (int)(newY < LeftBottom.Y ? LeftBottom.Y : newY))
                );
            }
            else
            {
                var newX = (int)(currentPos.X + minDist);
                var newY = (int)(currentPos.Y + minDist);
                return new PointF(
                    rand.Next((int)(newX > RightTop.X ? RightTop.X : newX), (int)RightTop.X),
                    rand.Next((int)(newY > RightTop.Y ? RightTop.Y : newY), (int)RightTop.Y)
                );
            }
        }

        public bool InZone(double x, double y)
        {
            return RightTop.X >= x && RightTop.Y >= y && LeftBottom.X <= x && LeftBottom.Y <= y;
        }

        public double BaseThreatIndex => AdjacentZones.Length * BASE_THREAT;
    }
}
