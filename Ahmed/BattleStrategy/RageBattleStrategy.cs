using Robocode;
using Robocode.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Romanchuk.Helpers;
using Romanchuk.MoveStrategy;

namespace Romanchuk.BattleStrategy
{
    public class RageBattleStrategy<T> : IBattleStrategy where T : AdvancedRobot
    {
        public Enemy CurrentTarget { get; private set; }

        public IEnumerable<Enemy> Enemies = new Enemy[]{};
        public IMoveStrategy MoveStrategy;

        private readonly AdvancedRobot _robot;
        private IEnumerable<HitByBulletEvent> _bulletHits;

        public RageBattleStrategy(T robot)
        {
            _robot = robot;
        }

        public void ChangeColor(ref int colorIteration)
        {
            const double frequency = .3;
            colorIteration++;
            if (colorIteration > 32) { colorIteration = 0; }
            _robot.SetAllColors(Color.FromArgb(
                (byte)(Math.Sin(frequency * colorIteration + 0) * 127 + 128),
                (byte)(Math.Sin(frequency * colorIteration + 2) * 127 + 128),
                (byte)(Math.Sin(frequency * colorIteration + 4) * 127 + 128)
            ));
        }

        public void AttachHitByBulletEvents(IEnumerable<HitByBulletEvent> bulletHits)
        {
            _bulletHits = bulletHits;
        }

        public void Move()
        {
            /*
            if (MoveStrategy == null)
            {
                MoveStrategy = new SafeZoneMoveStrategy(_robot);
            }


            var lastHitsByBullet = _bulletHits.Where(h => (_robot.Time - h.Time) < 5);

            var dest = MoveStrategy.GetDestination(Enemies, CurrentTarget, lastHitsByBullet.Count() >= 2);

            double absDeg = MathHelpers.AbsoluteBearingDegrees(_robot.X, _robot.Y, dest.X, dest.Y);
            var angleToTurn = MathHelpers.TurnRightOptimalAngle(_robot.Heading, absDeg);

            if (Math.Abs(_robot.X - dest.X) > _robot.Width*2 || Math.Abs(_robot.Y - dest.Y) > _robot.Width * 2)
            {
                DirectionMove(angleToTurn, MoveStrategy.UnsafeMovement ? Rules.MAX_VELOCITY : Rules.MAX_VELOCITY / 2);
            }
            else
            {
                DirectionMove(angleToTurn, 2);
            }

            if (angleToTurn > 180)
            {
                _robot.SetAhead(0);
            }*/
            if (MoveStrategy == null)
            {
                MoveStrategy = new SpiralMoveStrategy(_robot);
            }
            var dest = MoveStrategy.GetDestination(Enemies, CurrentTarget, false);
            double absDeg = MathHelpers.AbsoluteBearingDegrees(_robot.X, _robot.Y, dest.X, dest.Y);
            var angleToTurn = MathHelpers.TurnRightOptimalAngle(_robot.Heading, absDeg);
            if (Math.Abs(angleToTurn) > 180)
            {
                DirectionMove(angleToTurn, 0);
            } else
            {
                DirectionMove(angleToTurn, MoveStrategy.UnsafeMovement ? Rules.MAX_VELOCITY : Rules.MAX_VELOCITY / 2);
            }
            
        }


        public void ChooseTarget(IEnumerable<Enemy> enemies)
        {
            Enemies = enemies ?? throw new ArgumentNullException(nameof(enemies));
            if (CurrentTarget != null)
            {
                CurrentTarget = Enemies.FirstOrDefault(e => e.Name.Equals(CurrentTarget.Name));
            }

            if (Enemies.Count() == 1)
            {
                CurrentTarget = Enemies.First();
                return;
            }

            var minDistance = enemies.Min(e => e.Instance.Distance);
            var closestEnemies = enemies.Where(e => (e.Instance.Distance - minDistance) < 200);
            var minEnergy = closestEnemies.Min(e => e.Instance.Energy);
            var selectedTargets = closestEnemies.Where(e => (e.Instance.Energy - minEnergy) < 10).ToArray();
            var selectedTargetsGunDiff = selectedTargets.Select(e =>
            {
                double absDeg = MathHelpers.AbsoluteBearingDegrees(_robot.X, _robot.Y, e.X, e.Y);
                var diff = Math.Abs(MathHelpers.TurnRightOptimalAngle(_robot.GunHeading, absDeg));
                return new
                {
                    e,
                    diff
                };

            });
            var minGunDiff = selectedTargetsGunDiff.Min(e => e.diff);
            var criteria = new
            {
                energy = selectedTargets.Select(e => 1 - 1 / (e.Instance.Energy / minEnergy)).ToArray(),
                distance = selectedTargets.Select(e => 1 - 1 / (e.Instance.Distance / minDistance)).ToArray(),
                gunTurnDiff = selectedTargetsGunDiff.Select(e => 1 - 1 / (e.diff / minGunDiff)).ToArray()
            };
            
            var optimalTargets = new (Enemy e, double c)[selectedTargets.Length];
            for (var i = 0; i < selectedTargets.Length; i++)
            {
                var totalCriteria = criteria.energy[i] + criteria.distance[i] + criteria.gunTurnDiff[i]*0.2;
                optimalTargets[i].e = selectedTargets[i];
                optimalTargets[i].c = totalCriteria;
            }
            var orderedOptTargets = optimalTargets.OrderBy(o => o.c);

            
            if (!orderedOptTargets.Take(2).Any(t => t.e.Name.Equals(CurrentTarget?.Name))) {
                CurrentTarget = orderedOptTargets.Select(o => o.e).First();
            }
        }
    

        public void ResetTarget()
        {
            CurrentTarget = null;
        }

        public void Shoot()
        {
            if (CurrentTarget == null)
            {
                return;
            }
            if (_robot.Energy < 0.2)
            {
                return;
            }

            double bulletPower = CalcBulletPower(CurrentTarget.Instance.Energy, CurrentTarget.Instance.Distance);
            long timeToHitEnemy = (long)(CurrentTarget.Instance.Distance / MathHelpers.CalculateBulletSpeed(bulletPower));
            _robot.Out.WriteLine("====== AIMING =======");
            double futureX = CurrentTarget.GetFutureX(timeToHitEnemy);
            double futureY = CurrentTarget.GetFutureY(timeToHitEnemy);
            double absDeg = MathHelpers.AbsoluteBearingDegrees(_robot.X, _robot.Y, futureX, futureY);

            var currentGunHeadingRemaining = _robot.GunTurnRemaining;
            var angleToTurn = MathHelpers.TurnRightOptimalAngle(_robot.GunHeading, absDeg);

            _robot.Out.WriteLine($"POSITION My: {_robot.X}, {_robot.Y}; Enemy: {futureX}, {futureY}");
            _robot.Out.WriteLine($"Gun heading: {_robot.GunHeading}; Abs bearing: {absDeg}; Gun turn deg: {angleToTurn}");


            _robot.SetTurnGunRight(angleToTurn);

            
            _robot.Out.WriteLine("====== SHOOTING =======");
            _robot.Out.WriteLine($"Turn Gun:  {currentGunHeadingRemaining}; Distance:  {CurrentTarget.Instance.Distance}; Gun Heat: {_robot.GunHeat}");
            
            if (CurrentTarget.Instance.Distance > 500 && Math.Abs(currentGunHeadingRemaining) > 0.2)
            {
                _robot.Out.WriteLine("Skip shoot");
                return;
            }
            if (CurrentTarget.Instance.Distance > 300 && Math.Abs(currentGunHeadingRemaining) > 0.5)
            {
                _robot.Out.WriteLine("Skip shoot");
                return;
            }
            if (CurrentTarget.Instance.Distance > 200 && Math.Abs(currentGunHeadingRemaining) > 0.7)
            {
                _robot.Out.WriteLine("Skip shoot");
                return;
            }
            if (CurrentTarget.Instance.Distance > 100 && Math.Abs(currentGunHeadingRemaining) > 1)
            {
                _robot.Out.WriteLine("Skip shoot");
                return;
            }
            if (CurrentTarget.Instance.Distance > 0 && Math.Abs(currentGunHeadingRemaining) > 2)
            {
                _robot.Out.WriteLine("Skip shoot");
                return;
            }
            
            if (_robot.GunHeat == 0)
            {
                _robot.SetFire(bulletPower);
            }
            else
            {
                _robot.Out.WriteLine("Skip shoot");
            }
        }


        private double CalcBulletPower(double enemyEnergy, double enemyDistance)
        {
            if (enemyEnergy - 2 > Rules.MAX_BULLET_POWER * 4 && _robot.Energy > Rules.MAX_BULLET_POWER * 3 && enemyDistance < 2 * _robot.Width)
            {
                return Rules.MAX_BULLET_POWER;
            }
            else if (enemyEnergy > 40 && _robot.Energy > 4 && enemyDistance < 200)
            {
                return 3;
            }
            else if (enemyEnergy > 10 && _robot.Energy > 3)
            {
                return 2;
            }
            else if (enemyEnergy > 4 && _robot.Energy > 1)
            {
                return 1;
            }
            else if (enemyEnergy > 2 && _robot.Energy > 0.5)
            {
                return .5;
            }
            else if (enemyEnergy > .4 && _robot.Energy > 0.2)
            {
                return .1;
            }
            return .1;
        }

        private void DirectionMove(double forwardAngleTurn, double velocity)
        {
            //if (Math.Abs(forwardAngleTurn) < 180)
            //{
                _robot.SetTurnRight(forwardAngleTurn);
                _robot.SetAhead(velocity);
                /*
            }
            else
            {
                _robot.SetTurnRight(180 - Math.Abs(forwardAngleTurn));
                _robot.SetBack(velocity);
            }*/
        }
    }
}
