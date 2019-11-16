﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Robocode;
using Romanchuk.Helpers;

namespace Romanchuk.MoveStrategy
{

    class Zone
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
                    else if (e.Instance.Energy < 20 && EnemiesInZone.Count > 1)
                    {
                        enemiesThreat += -BASE_THREAT/2;
                    }
                    else if (e.Instance.Energy < 30)
                    {
                        enemiesThreat += BASE_THREAT/2;
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

                double adjacentZonesThreat = AdjacentZones.Select(z => (double) z.EnemiesInZone.Count)
                    .Aggregate((c, r) => c * 0.1);

                return BaseThreatIndex + enemiesThreat + adjacentZonesThreat;
            }
        }

        public PointF GetCenterPoint()
        {
            return new PointF(RightTop.X - (RightTop.X - LeftBottom.X)/2, RightTop.Y - (RightTop.Y - LeftBottom.Y)/2);
        }

        public PointF GetRandomPoint()
        {
            var rand = new Random();
            return new PointF(rand.Next((int)LeftBottom.X, (int)RightTop.X), rand.Next((int)LeftBottom.Y, (int)RightTop.Y));
        }

        public bool InZone(double x, double y)
        {
            return RightTop.X >= x && RightTop.Y >= y&& LeftBottom.X <= x && LeftBottom.Y <= y;
        }

        public double BaseThreatIndex => AdjacentZones.Length * BASE_THREAT;
    }

    public class SafeZoneMoveStrategy : IMoveStrategy
    {
        private readonly Zone[] _zones = new Zone[9];
        private Zone DestinationZone = null;

        public PointF DestinationPoint = new PointF(0,0);
        

        public SafeZoneMoveStrategy(double battleFieldHeight, double battleFieldWidth)
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

            // Set AdjacentZones 
            _zones[0].AdjacentZones = new[] { _zones[3], _zones[1]};
            _zones[1].AdjacentZones = new[] { _zones[0], _zones[2], _zones[4] };
            _zones[2].AdjacentZones = new[] { _zones[1], _zones[5] };
            _zones[3].AdjacentZones = new[] { _zones[0], _zones[6], _zones[4] };
            _zones[4].AdjacentZones = new[] { _zones[1], _zones[3], _zones[7], _zones[5] };
            _zones[5].AdjacentZones = new[] { _zones[4], _zones[8], _zones[2] };
            _zones[6].AdjacentZones = new[] { _zones[3], _zones[7] };
            _zones[7].AdjacentZones = new[] { _zones[6], _zones[4], _zones[8] };
            _zones[8].AdjacentZones = new[] { _zones[7], _zones[5] };
        }

        public PointF SetDestination(IEnumerable<Enemy> enemies, Robot myRobot)
        {
            ResetZonesData();

            foreach (var e in enemies)
            {
                foreach (var z in _zones)
                {
                    if (z.InZone(e.X, e.Y))
                    {
                        z.EnemiesInZone.Add(e);
                        break;
                    }
                }
            }

            var zones = _zones.Select(e => new
                           {
                               e,
                               distance = MathHelpers.CalculateDistance(myRobot.X, myRobot.Y, e.GetCenterPoint().X, e.GetCenterPoint().Y)
                           });

            var minDistZone = zones
                    .OrderBy(z => z.e.ThreatIndex)
                    .ThenBy(z => z.distance)                 
                    .First();

            DestinationZone = DestinationZone ?? minDistZone.e;

            PointF point = DestinationPoint;
            if (enemies.Count() < 3)
            {
                DestinationZone = zones.OrderByDescending(z => z.e.EnemiesInZone.Count).First().e.AdjacentZones.First();
                point = DestinationZone.GetRandomPoint();
            }
            else if (minDistZone.e != DestinationZone)
            {
                DestinationZone = minDistZone.e;
                point = DestinationZone.GetRandomPoint();
            }
            else if (Math.Abs(DestinationPoint.X - myRobot.X) <= 10 && Math.Abs(DestinationPoint.Y - myRobot.Y) <= 10)
            {
                point = DestinationZone.GetRandomPoint();
            }
            DestinationPoint = CorrectPointOnBorders(point, myRobot.BattleFieldWidth, myRobot.BattleFieldHeight, (float)(myRobot.Width*2));
            return DestinationPoint;
        }

        private void ResetZonesData()
        {
            for (var i = 0; i < _zones.Length; i++)
            {
                _zones[i].EnemiesInZone = new List<Enemy>();
            }
        }

        private PointF CorrectPointOnBorders(PointF point, double maxX, double maxY, float safeBorderDist)
        {
            float newX = point.X;
            float newY = point.Y;
            if (point.X < safeBorderDist)
            {
                newX = safeBorderDist;
            }
            if (point.Y < safeBorderDist)
            {
                newY = safeBorderDist;
            }
            if (maxX - point.X < safeBorderDist)
            {
                newX = safeBorderDist;
            }
            if (maxY - point.Y < safeBorderDist)
            {
                newY = safeBorderDist;
            }
            return new PointF(newX, newY);
        }

    }
}
