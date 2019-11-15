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

        private long LastTimeTargetChanged = -1;

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

        public void Move()
        {
            if (MoveStrategy == null)
            {
                MoveStrategy = new SafeZoneMoveStrategy(_robot.BattleFieldHeight, _robot.BattleFieldWidth);
            }

            var dest = MoveStrategy.SetDestination(Enemies, _robot);
            double absDeg = ShootHelpers.AbsoluteBearingDegrees(_robot.X, _robot.Y, dest.X, dest.Y);

            var angleToTurn = absDeg - _robot.Heading;
            _robot.SetTurnRight(angleToTurn);

            if (Math.Abs(_robot.X - dest.X) < 10 && Math.Abs(_robot.Y - dest.Y) < 10)
            {
                return;
            }

            // var nextHeading = Robot.HeadingRadians + turnRadians;
            if (/*lastTimeBeingHit != -1 && Robot.Time - lastTimeBeingHit < 24 || */_robot.Energy < 15 || (CurrentTarget != null && Math.Abs(CurrentTarget.Instance.BearingRadians) < 0.5))
            {
                if (!_isTurning)
                {
                    _robot.SetAhead(Rules.MAX_VELOCITY);
                }
                else
                {
                    _robot.SetAhead(Rules.MAX_VELOCITY / 2);
                }
            }
            else
            {
                _robot.SetAhead(Rules.MAX_VELOCITY / 2);
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
            /*
            if (LastTimeTargetChanged != -1 && Enemies.Any(t => t.Name.Equals(CurrentTarget?.Name)) && (_robot.Time - LastTimeTargetChanged) > 3)
            {
                return;
            }
            */

            var minDistance = enemies.Min(e => e.Instance.Distance);
            var closestEnemies = enemies.Where(e => (e.Instance.Distance - minDistance) < 200);
            var minEnergy = closestEnemies.Min(e => e.Instance.Energy);
            var selectedTargets = closestEnemies.Where(e => (e.Instance.Energy - minEnergy) < 10).ToArray();
            var selectedTargetsGunDiff = selectedTargets.Select(e =>
            {
                double absDeg = ShootHelpers.AbsoluteBearingDegrees(_robot.X, _robot.Y, e.X, e.Y);
                var diff = Math.Abs(_robot.GunHeading - absDeg);
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
                LastTimeTargetChanged = _robot.Time;
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
            long timeToHitEnemy = (long)(CurrentTarget.Instance.Distance / ShootHelpers.CalculateBulletSpeed(bulletPower));

            double futureX = CurrentTarget.GetFutureX(timeToHitEnemy);
            double futureY = CurrentTarget.GetFutureY(timeToHitEnemy);
            double absDeg = ShootHelpers.AbsoluteBearingDegrees(_robot.X, _robot.Y, futureX, futureY);

            var currentGunHeadingRemaining = _robot.GunTurnRemaining;
            var angleToTurn = absDeg - _robot.GunHeading;
            _robot.SetTurnGunRight(angleToTurn);


            _robot.Out.WriteLine("===================");
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

        private bool _isTurning = false;


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
    }
}
