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
        public Enemy[] Enemies = {};
        public IMoveStrategy MoveStrategy;

        private readonly AdvancedRobot _robot;        

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

            //var turnAngle = Utils.NormalAbsoluteAngle(move(_robot.X, _robot.Y, _robot.HeadingRadians, 1, 1));
            var dest = MoveStrategy.SetDestination(Enemies, _robot);
            double absDeg = ShootHelpers.AbsoluteBearingDegrees(_robot.X, _robot.Y, dest.X, dest.Y);

            var angleToTurn = absDeg - _robot.Heading;
            _robot.SetTurnRight(angleToTurn);

            if (Math.Abs(_robot.X - dest.X) < 10 && Math.Abs(_robot.Y - dest.Y) < 10)
            {
                //_robot.Stop();
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
            if (enemies == null) { throw new ArgumentNullException(nameof(enemies)); }

            Enemies = enemies
                .OrderByDescending(e => e.Instance.Energy)
                .ThenByDescending(e => e.Instance.Distance)
                .ToArray();
            Enemy currentTargetInEnemies = null;
            if (CurrentTarget != null)
            {
                currentTargetInEnemies = Enemies.FirstOrDefault(e => e.Name.Equals(CurrentTarget.Name));
            }
            CurrentTarget = currentTargetInEnemies ?? Enemies.First();

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

            double bulletPower = CalcBulletPower(CurrentTarget.Instance.Energy, CurrentTarget.Instance.Distance);
            long timeToHitEnemy = (long)(CurrentTarget.Instance.Distance / ShootHelpers.CalculateBulletSpeed(bulletPower));

            double futureX = CurrentTarget.GetFutureX(timeToHitEnemy);
            double futureY = CurrentTarget.GetFutureY(timeToHitEnemy);
            double absDeg = ShootHelpers.AbsoluteBearingDegrees(_robot.X, _robot.Y, futureX, futureY);

            var angleToTurn = absDeg - _robot.GunHeading;
            _robot.SetTurnGunRight(angleToTurn);

 
            if (_robot.GunHeat == 0 && Math.Abs(_robot.GunTurnRemaining) < 0.5)
            {
                _robot.SetFire(bulletPower);
            }
            
        }

        private bool _isTurning = false;

        public double move(double x, double y, double heading, int orientation, int smoothTowardEnemy)
        {
            _isTurning = false;
            var WALL_STICK = 100;
            if (_robot.Velocity >= 6)
                WALL_STICK += 80;
            var angle = 0.005;

            double halfOfRobot = _robot.Width / 2;
            double distanceToWallX = Math.Min(x - halfOfRobot, _robot.BattleFieldWidth - x - halfOfRobot);
            double distanceToWallY = Math.Min(y - halfOfRobot, _robot.BattleFieldHeight - y - halfOfRobot);

            double nextX = x + (Math.Sin(angle) * WALL_STICK);
            double nextY = y + (Math.Cos(angle) * WALL_STICK);

            double nextDistanceToWallX = Math.Min(nextX - halfOfRobot, _robot.BattleFieldWidth - nextX - halfOfRobot);
            double nextDistanceToWallY = Math.Min(nextY - halfOfRobot, _robot.BattleFieldHeight - nextY - halfOfRobot);

            double adjacent = 0;

            if (nextDistanceToWallY <= WALL_STICK && nextDistanceToWallY < nextDistanceToWallX)
            {
                // wall smooth North or South wall
                angle = (angle + (Math.PI / 2));/* / Math.PI) * Math.PI;*/
                adjacent = Math.Abs(distanceToWallY);
                _isTurning = true;
            }
            else if (nextDistanceToWallX <= WALL_STICK && nextDistanceToWallX < nextDistanceToWallY)
            {
                // wall smooth East or West wall
                angle = (((angle / Math.PI)) * Math.PI) + (Math.PI / 2);
                adjacent = Math.Abs(distanceToWallX);
                _isTurning = true;
            }
            else if (distanceToWallY + halfOfRobot <= WALL_STICK || distanceToWallX + halfOfRobot <= WALL_STICK)
            {
                if (_robot.HeadingRadians < Math.PI / 2)
                {
                    angle = (angle - (Math.PI / 2));
                }
                else if (_robot.HeadingRadians > Math.PI / 2 && _robot.HeadingRadians < Math.PI)
                {
                    angle = (angle + (Math.PI / 2));
                }
                else if (_robot.HeadingRadians > Math.PI && _robot.HeadingRadians < Math.PI + Math.PI / 2)
                {
                    angle = (angle - (Math.PI / 2));
                }
                else if (_robot.HeadingRadians > Math.PI && _robot.HeadingRadians < Math.PI + Math.PI / 2)
                {
                    angle = (angle + (Math.PI / 2));
                }
                _isTurning = true;
            }

            return angle; // you may want to normalize this
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
    }
}
