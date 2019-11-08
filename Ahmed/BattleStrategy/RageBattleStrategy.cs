using Robocode;
using Robocode.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Romanchuk.BattleStrategy
{
    public class RageBattleStrategy<T> : IBattleStrategy where T : AdvancedRobot
    {
        public Target Target { get; }

        private readonly AdvancedRobot Robot;        

        public RageBattleStrategy(T robot)
        {
            Robot = robot;
            Target = new Target(Robot);
        }

        public void ChangeColor(ref int colorIteration)
        {
            const double frequency = .3;
            colorIteration++;
            if (colorIteration > 32) { colorIteration = 0; }
            Robot.SetAllColors(Color.FromArgb(
                (byte)(Math.Sin(frequency * colorIteration + 0) * 127 + 128),
                (byte)(Math.Sin(frequency * colorIteration + 2) * 127 + 128),
                (byte)(Math.Sin(frequency * colorIteration + 4) * 127 + 128)
            ));
        }

        public void Move()
        {
            var turnAngle = Utils.NormalAbsoluteAngle(move(Robot.X, Robot.Y, Robot.HeadingRadians, 1, 1));
            Robot.SetTurnRight(turnAngle);
            // var nextHeading = Robot.HeadingRadians + turnRadians;
            if (/*lastTimeBeingHit != -1 && Robot.Time - lastTimeBeingHit < 24 || */Robot.Energy < 15 || (!Target.None && Math.Abs(Target.Instance.BearingRadians) < 0.5))
            {
                if (!isTurning)
                {
                    Robot.SetAhead(Rules.MAX_VELOCITY);
                }
                else
                {
                    Robot.SetAhead(Rules.MAX_VELOCITY / 2);
                }
            }
            else
            {
                Robot.SetAhead(Rules.MAX_VELOCITY / 2);
            }
        }


        public void ChooseTarget(IEnumerable<ScannedRobotEvent> enemies)
        {
            if (enemies == null)
            {
                throw new ArgumentNullException(nameof(enemies));
            }
            var orderedEnemies = enemies
                   .OrderByDescending(e => e.Energy)
                   .ThenByDescending(e => e.Distance);

            Target.Set(orderedEnemies.First());
        }

        public void ResetTarget()
        {
            Target.Reset();
        }

        public void Shoot()
        {
            if (Target.None)
            {
                return;
            }
            var turnGunRadians = GetDiffTargetAndGunRadians(Target.Instance);
            Robot.SetTurnGunRightRadians(turnGunRadians);
            if (Math.Abs(turnGunRadians) < 0.15)
            {
                if (Robot.GunHeat == 0)
                {
                    SetAdjustedFire(Target.Instance.Energy, Target.Instance.Distance);
                }
            }
        }

        private bool isTurning = false;

        public double move(double x, double y, double heading, int orientation, int smoothTowardEnemy)
        {
            isTurning = false;
            var WALL_STICK = 100;
            if (Robot.Velocity >= 6)
                WALL_STICK += 80;
            var angle = 0.005;

            double halfOfRobot = Robot.Width / 2;
            double distanceToWallX = Math.Min(x - halfOfRobot, Robot.BattleFieldWidth - x - halfOfRobot);
            double distanceToWallY = Math.Min(y - halfOfRobot, Robot.BattleFieldHeight - y - halfOfRobot);

            double nextX = x + (Math.Sin(angle) * WALL_STICK);
            double nextY = y + (Math.Cos(angle) * WALL_STICK);

            double nextDistanceToWallX = Math.Min(nextX - halfOfRobot, Robot.BattleFieldWidth - nextX - halfOfRobot);
            double nextDistanceToWallY = Math.Min(nextY - halfOfRobot, Robot.BattleFieldHeight - nextY - halfOfRobot);

            double adjacent = 0;

            if (nextDistanceToWallY <= WALL_STICK && nextDistanceToWallY < nextDistanceToWallX)
            {
                // wall smooth North or South wall
                angle = (angle + (Math.PI / 2));/* / Math.PI) * Math.PI;*/
                adjacent = Math.Abs(distanceToWallY);
                isTurning = true;
            }
            else if (nextDistanceToWallX <= WALL_STICK && nextDistanceToWallX < nextDistanceToWallY)
            {
                // wall smooth East or West wall
                angle = (((angle / Math.PI)) * Math.PI) + (Math.PI / 2);
                adjacent = Math.Abs(distanceToWallX);
                isTurning = true;
            }
            else if (distanceToWallY + halfOfRobot <= WALL_STICK || distanceToWallX + halfOfRobot <= WALL_STICK)
            {
                if (Robot.HeadingRadians < Math.PI / 2)
                {
                    angle = (angle - (Math.PI / 2));
                }
                else if (Robot.HeadingRadians > Math.PI / 2 && Robot.HeadingRadians < Math.PI)
                {
                    angle = (angle + (Math.PI / 2));
                }
                else if (Robot.HeadingRadians > Math.PI && Robot.HeadingRadians < Math.PI + Math.PI / 2)
                {
                    angle = (angle - (Math.PI / 2));
                }
                else if (Robot.HeadingRadians > Math.PI && Robot.HeadingRadians < Math.PI + Math.PI / 2)
                {
                    angle = (angle + (Math.PI / 2));
                }
                isTurning = true;
            }

            return angle; // you may want to normalize this
        }


        private void SetAdjustedFire(double enemyEnergy, double enemyDistance)
        {
            if (enemyEnergy - 2 > Rules.MAX_BULLET_POWER * 4 && Robot.Energy > Rules.MAX_BULLET_POWER * 3 && enemyDistance < 2 * Robot.Width)
            {
                Robot.SetFire(Rules.MAX_BULLET_POWER);
            }
            else if (enemyEnergy > 40 && Robot.Energy > 4 && enemyDistance < 200)
            {
                Robot.SetFire(3);
            }
            else if (enemyEnergy > 10 && Robot.Energy > 3)
            {
                Robot.SetFire(2);
            }
            else if (enemyEnergy > 4 && Robot.Energy > 1)
            {
                Robot.SetFire(1);
            }
            else if (enemyEnergy > 2 && Robot.Energy > 0.5)
            {
                Robot.SetFire(.5);
            }
            else if (enemyEnergy > .4 && Robot.Energy > 0.2)
            {
                Robot.SetFire(.1);
            }
        }

        private double GetDiffTargetAndGunRadians(ScannedRobotEvent target)
        {
            var normAbsBearing = Robot.HeadingRadians + target.BearingRadians;

            var targetNextHeadingRadians =
                Robot.HeadingRadians + target.HeadingRadians + (target.Velocity / 10000);
            Robot.Out.Write("Predicted heading: " + targetNextHeadingRadians);

            var angle = Utils.NormalRelativeAngle(normAbsBearing - Robot.GunHeadingRadians);
            var predAngleKoef = (target.Velocity / Math.PI);
            return Utils.NormalRelativeAngle(angle + angle * predAngleKoef);
        }
    }
}
