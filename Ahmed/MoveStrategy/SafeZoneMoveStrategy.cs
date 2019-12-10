using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Robocode;
using Romanchuk.Helpers;
using Romanchuk.MoveStrategy.Class;

namespace Romanchuk.MoveStrategy
{

    public class SafeZoneMoveStrategy : IMoveStrategy
    {
        private readonly AdvancedRobot _myRobot;
        private readonly Zone[] _zones = new Zone[9];
        private Zone _destinationZone;

        private Zone CurrentZone
        {
            get { return _zones.First(z => z.InZone(_myRobot.X, _myRobot.Y)); }
        }

        public PointF DestinationPoint = new PointF(0,0);

        public bool UnsafeMovement
        {
            get
            {
                if (_destinationZone == null)
                {
                    return false;
                }
                return !_destinationZone.InZone(_myRobot.X, _myRobot.Y);
            }
        }
        

        public SafeZoneMoveStrategy(AdvancedRobot robot)
        {
            _myRobot = robot;
            const int zonesInLine = 3;
            var zoneWidth = _myRobot.BattleFieldWidth / zonesInLine;
            var zoneHeight = _myRobot.BattleFieldHeight / zonesInLine;

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

        public PointF GetDestination(IEnumerable<Enemy> enemies, Enemy currentTarget, bool forceChangeDirection)
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

            Zone[] exeptions = {};
            if (forceChangeDirection)
            {
                exeptions = new[] { CurrentZone, _destinationZone };
            }
            var zones = _zones.Except(exeptions).Select(e => new
                           {
                               e,
                               distance = MathHelpers.CalculateDistance(_myRobot.X, _myRobot.Y, e.GetCenterPoint().X, e.GetCenterPoint().Y)
                           });

            

            var minDistZone = zones
                    .OrderBy(z => z.e.ThreatIndex)
                    .ThenBy(z => z.distance)                 
                    .First();

            _destinationZone = _destinationZone ?? minDistZone.e;

            PointF point = DestinationPoint;
            
            if (minDistZone.e != _destinationZone)
            {
                _destinationZone = minDistZone.e;
                point = _destinationZone.GetRandomPoint();
            } else if (Math.Abs(DestinationPoint.X - _myRobot.X) <= 80 && Math.Abs(DestinationPoint.Y - _myRobot.Y) <= 80)
            {
                point = _destinationZone.GetPointExcept(new PointF((float)_myRobot.X, (float)_myRobot.Y), 100);
            } else
            {
                _destinationZone = zones
                    .OrderByDescending(z => z.e.EnemiesInZone.Count)
                    .First().e.AdjacentZones
                        .OrderBy(az => az.ThreatIndex)
                        .ThenBy(az => MathHelpers.CalculateDistance(_myRobot.X, _myRobot.Y, az.GetCenterPoint().X, az.GetCenterPoint().Y))
                        .First();
                point = _destinationZone.GetRandomPoint();
            }
            DestinationPoint = MathHelpers.CorrectPointOnBorders(point, _myRobot.BattleFieldWidth, _myRobot.BattleFieldHeight, (float)(_myRobot.Width*2));
            return DestinationPoint;
        }

        public void Move(IEnumerable<Enemy> enemies, Enemy currentTarget, bool underAttack)
        {
            var dest = GetDestination(enemies, currentTarget, false);
            var angleToTurn = MathHelpers.TurnRobotToPoint(_myRobot, dest);

            _myRobot.SetTurnRight(angleToTurn);
            double velocity = UnsafeMovement || underAttack ? Config.MaxDistancePerTurn : Config.MaxDistancePerTurn / 4;
            if (Math.Abs(angleToTurn) > 90)
            {
                velocity = Config.MaxDistancePerTurn / 4;
            }
            _myRobot.SetAhead(velocity);
        }
        private void ResetZonesData()
        {
            foreach (var z in _zones)
            {
                z.EnemiesInZone = new List<Enemy>();
            }
        }

    }
}
