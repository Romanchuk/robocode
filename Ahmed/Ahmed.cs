using Robocode;
using Robocode.Util;
using System.Drawing;
using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reflection;
using System.Security;

namespace Romanchuk
{
    public class Ahmed : AdvancedRobot
    {
        private readonly IDictionary<string, ScannedRobotEvent> enemies = new Dictionary<string, ScannedRobotEvent>();

        private ScannedRobotEvent CurrentTarget = null;
        private ScannedRobotEvent RageTarget = null;
        private BehaviorSubject<int> Observable;

        private long lastTimeBeingHit = -1;

        override public void Run() {
            IsAdjustGunForRobotTurn = true;
            IsAdjustRadarForRobotTurn = true;

            
            int colorIteration = 1;
            SetAllColors(Color.Black);

            Observable = new BehaviorSubject<int>(1);

            while (true) {
                SetAllColors(Color.Black);
                // Observable.OnNext(3);
                Out.WriteLine($"----------------------------");
                
                SetTurnRadarRight(Rules.RADAR_TURN_RATE);

                var turnRadians = Utils.NormalAbsoluteAngle(move(X, Y, HeadingRadians, 1, 1));
                var nextHeading = HeadingRadians + turnRadians;


                var lifeEnemies = enemies
                    .Select(d => d.Value)
                    .Where(e => e.Energy >= 0)
                    .OrderByDescending(e => e.Energy)
                    .ThenByDescending(e => e.Distance)
                    .Take(Others);
                RageTarget = lifeEnemies.FirstOrDefault(e => e.Name.Equals(RageTarget?.Name))
                        ?? lifeEnemies
                            .Where(e => e.Distance < 500)
                            .Where(e => e.Energy < 40 || e.Energy < 45 && Others == 1)
                            .OrderBy(e => e.Energy)
                            .FirstOrDefault();
                
                if (RageTarget == null || Energy <= 10) {

                    var target = lifeEnemies.OrderBy(e => e.Distance).FirstOrDefault();
                    if (target != null)
                    {
                        bool targetChanged = CurrentTarget != null && CurrentTarget != target;
                        CurrentTarget = target;
                        var normAbsBearing = HeadingRadians + target.BearingRadians;
                        var b = Utils.NormalRelativeAngle(normAbsBearing - GunHeadingRadians);
                        Debug.WriteLine($"Enemy ({target.Name}) Abs Bearing Norm: " + normAbsBearing);
                        Debug.WriteLine("Gun Heading: " + GunHeadingRadians);
                        Out.WriteLine($"Enemy ({target.Name}) Abs Bearing Norm: " + normAbsBearing);
                        Out.WriteLine($"Next Heading: {nextHeading}");
                        Out.WriteLine($"Heading: {HeadingRadians}");
                        Out.WriteLine("Gun Heading: " + GunHeadingRadians);
                        
                        Debug.WriteLine("Turn Gun Radians: " + b);
                        Out.WriteLine("Turn Gun Radians: " + b);
                        var turnGunRadians = GetGunTurnRightRadians(target.BearingRadians);
                        SetTurnGunRightRadians(turnGunRadians);
                        
                        if (GunHeat == 0 && Math.Abs(turnGunRadians) < 0.3)
                        {
                            var firePower = 0.5;
                            if (CurrentTarget.Distance < 400 && Energy > 12)
                            {
                                firePower = 2.0;
                            }
                            else if (CurrentTarget.Distance < 180 && Energy > 20)
                            {
                                firePower = Rules.MAX_BULLET_POWER/2;
                            }
                            else if (CurrentTarget.Distance < 100 && Energy > 24 && Math.Abs(turnGunRadians) < 0.1 || CurrentTarget.Velocity == 0 && CurrentTarget.Distance < 200)
                            {
                                firePower = Rules.MAX_BULLET_POWER;
                            }             
                            if (Energy <= Rules.MAX_BULLET_POWER)
                            {
                                firePower = 0.2;
                            }
                            var diff = Energy - firePower;
                            if (diff >= 1)
                            {
                                SetFire(firePower);
                            } else if (Math.Abs(turnGunRadians) < 0.05 && Energy > 0.1)
                            {
                                SetFire(0.1);
                            }

                        }
                    }

                    SetTurnRight(turnRadians);
                } else {
                    Out.WriteLine($"Enemy ({RageTarget.Name})");
                    ChangeColor(ref colorIteration);
                    SetTurnRightRadians(RageTarget.BearingRadians);
                    var turnGunRadians = GetGunTurnRightRadians(RageTarget.BearingRadians);
                    SetTurnGunRightRadians(turnGunRadians);
                    if (Math.Abs(turnGunRadians) < 0.15)
                    {
                        SetAdjustedFire(RageTarget.Energy);
                    }                    
                }

                if (lastTimeBeingHit != -1 && Time - lastTimeBeingHit < 24 || Energy < 15 || (RageTarget !=null && Math.Abs(RageTarget.BearingRadians) < 0.5))
                {
                    SetAhead(Rules.MAX_VELOCITY);
                }
                else
                {
                    SetAhead(Rules.MAX_VELOCITY / 2);
                }

                Execute();
            }
        }


        override public void OnScannedRobot(ScannedRobotEvent ev)
        {
            var oldEvents = enemies.Where(e => Time - e.Value.Time > 12).ToList();
            foreach (var oe in oldEvents)
            {
                enemies.Remove(oe);
            }
            enemies[ev.Name] = ev;
        }

        public override void OnBulletHit(BulletHitEvent e)
        {
            CheckEnemyDied(e.VictimName, e.VictimEnergy);
        }

        public override void OnHitRobot(HitRobotEvent e)
        {
            CheckEnemyDied(e.Name, e.Energy);
            if (RageTarget == null)
                return;
            if (RageTarget.Name.Equals(e.Name) && e.Energy < 0)
            {
                RageTarget = null;
                return;
            }
        }

       
        public override void OnHitByBullet(HitByBulletEvent e)
        {
            lastTimeBeingHit = e.Time;
        }
        

        private void CheckEnemyDied(string name, double energy)
        {
            if (energy <= 0)
            {
                enemies.Remove(name);
            }
        }

        private void ChangeColor(ref int colorIteration)
        {
            const double frequency = .3;
            colorIteration++;
            if (colorIteration > 32) { colorIteration = 0; }
            SetAllColors(Color.FromArgb(
                (byte)(Math.Sin(frequency * colorIteration + 0) * 127 + 128),
                (byte)(Math.Sin(frequency * colorIteration + 2) * 127 + 128),
                (byte)(Math.Sin(frequency * colorIteration + 4) * 127 + 128)
            ));
        }

        public double move(double x, double y, double heading, int orientation, int smoothTowardEnemy)
        {
            var WALL_STICK = 100;
            if (Velocity >= 6)
                WALL_STICK += 70;
            var angle = 0.005;

            double halfOfRobot = Width / 2;
            double distanceToWallX = Math.Min(x - halfOfRobot, BattleFieldWidth - x - halfOfRobot);
            double distanceToWallY = Math.Min(y - halfOfRobot, BattleFieldHeight - y - halfOfRobot);

            double nextX = x + (Math.Sin(angle) * WALL_STICK);
            double nextY = y + (Math.Cos(angle) * WALL_STICK);
            
            double nextDistanceToWallX = Math.Min(nextX - halfOfRobot, BattleFieldWidth - nextX - halfOfRobot);
            double nextDistanceToWallY = Math.Min(nextY - halfOfRobot, BattleFieldHeight - nextY - halfOfRobot);

            double adjacent = 0;

            if (nextDistanceToWallY <= WALL_STICK && nextDistanceToWallY < nextDistanceToWallX)
            {
                // wall smooth North or South wall
                angle = (angle + (Math.PI / 2));/* / Math.PI) * Math.PI;*/
                adjacent = Math.Abs(distanceToWallY);
            }
            else if (nextDistanceToWallX <= WALL_STICK && nextDistanceToWallX < nextDistanceToWallY)
            {
                // wall smooth East or West wall
                angle = (((angle / Math.PI)) * Math.PI) + (Math.PI / 2);
                adjacent = Math.Abs(distanceToWallX);
            }
            else if (distanceToWallY + halfOfRobot <= WALL_STICK || distanceToWallX + halfOfRobot <= WALL_STICK)
            {
                if (HeadingRadians < Math.PI / 2)
                {
                    angle = (angle - (Math.PI / 2));
                }
                else if (HeadingRadians > Math.PI / 2 && HeadingRadians < Math.PI)
                {
                    angle = (angle + (Math.PI / 2));
                }
                else if (HeadingRadians > Math.PI && HeadingRadians < Math.PI + Math.PI / 2)
                {
                    angle = (angle - (Math.PI / 2));
                }
                else if (HeadingRadians > Math.PI && HeadingRadians < Math.PI + Math.PI / 2)
                {
                    angle = (angle + (Math.PI / 2));
                }
            }

            return angle; // you may want to normalize this
        }

        private double GetGunTurnRightRadians(double targetBearingRadians)
        {
            var normAbsBearing = HeadingRadians + targetBearingRadians;
            return Utils.NormalRelativeAngle(normAbsBearing - GunHeadingRadians);
        }

        private void SetAdjustedFire(double enemyEnergy)
        {
            if (this.GunHeat > 0)
            {
                return;
            }
            if (enemyEnergy > 16)
            {
                SetFire(3);
            }
            else if (enemyEnergy > 10)
            {
                SetFire(2);
            }
            else if (enemyEnergy > 4)
            {
                SetFire(1);
            }
            else if (enemyEnergy > 2)
            {
                SetFire(.5);
            }
            else if (enemyEnergy > .4)
            {
                SetFire(.1);
            }
        }


    }
}
