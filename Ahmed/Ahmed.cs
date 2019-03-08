using Robocode;
using Robocode.Util;
using System.Drawing;
using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace Romanchuk
{
    enum Strategy : byte
    {
        None = 0,
        Deathmatch = 1,
        Versus = 2,
        Rage = 4
    }
        
    public class Ahmed : AdvancedRobot
    {
        private readonly IDictionary<string, ScannedRobotEvent> enemies = new Dictionary<string, ScannedRobotEvent>();

        private byte CurrentStrategy = (byte) Strategy.None;
        private ScannedRobotEvent currentTarget = null;

        override public void Run() {
            IsAdjustGunForRobotTurn = true;
            IsAdjustRadarForRobotTurn = true;

            
            int colorIteration = 1;
            ChangeColor(ref colorIteration);
            

            while (true) {
                CurrentStrategy = Others == 1 ? (byte)Strategy.Versus : (byte)Strategy.Deathmatch;
                ChangeColor(ref colorIteration);
                SetTurnRadarRight(Rules.RADAR_SCAN_RADIUS);

                var turnRadians = Utils.NormalAbsoluteAngle(move(X, Y, HeadingRadians, 1, 1));
                var nextHeading = HeadingRadians + turnRadians;
                Out.WriteLine($"----------------------------");

                // ScannedSubject.AsObservable().
                var lifeEnemies = enemies
                    .Where(e => e.Value.Energy > 0)
                    .Select(d => d.Value)
                    .OrderBy(e => e.Energy)
                    .ThenBy(e => e.Distance)
                    .Take(Others);
                var target = lifeEnemies.FirstOrDefault();
                if (target != null)
                {
                    bool targetChanged = currentTarget != null && currentTarget != target;
                    currentTarget = target;
                    var normAbsBearing = HeadingRadians + target.BearingRadians;                    
                    Debug.WriteLine($"Enemy ({target.Name}) Abs Bearing Norm: " + normAbsBearing);
                    Debug.WriteLine("Gun Heading: " + GunHeadingRadians);
                    Out.WriteLine($"Enemy ({target.Name}) Abs Bearing Norm: " + normAbsBearing);
                    Out.WriteLine($"Next Heading: {nextHeading}");
                    Out.WriteLine($"Heading: {HeadingRadians}");
                    Out.WriteLine("Gun Heading: " + GunHeadingRadians);
                    var b = Utils.NormalRelativeAngle(normAbsBearing - GunHeadingRadians);
                    Debug.WriteLine("Turn Gun Radians: " + b);
                    Out.WriteLine("Turn Gun Radians: " + b);
                    SetTurnGunRightRadians(b);
                    /*
                    if (GunHeadingRadians > normAbsBearing)
                    {
                        SetTurnGunLeftRadians(b);
                    }
                    else
                    {
                        SetTurnGunRightRadians(b);
                    }*/
                    SetFire(1.0);
                }
                // Debug.WriteLine(Time);
                //Rules.RADAR_SCAN_RADIUS
                SetAhead(4.0);
                SetTurnRight(turnRadians);


                Execute();
            }
        }


        override public void OnScannedRobot(ScannedRobotEvent e)
        {
            enemies[e.Name] = e;
        }

        public override void OnBulletHit(BulletHitEvent e)
        {
            CheckEnemyDied(e.VictimName, e.VictimEnergy);
        }

        public override void OnHitRobot(HitRobotEvent e)
        {
            CheckEnemyDied(e.Name, e.Energy);
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

        private double WALL_STICK = 100;

        public double move(double x, double y, double heading, int orientation, int smoothTowardEnemy)
        {

            var angle = 0.005;

            // in Java, (-3 MOD 4) is not 1, so make sure we have some excess
            // positivity here
            // angle += Math.PI/3;
            double halfOfRobot = Width / 2;
            double distanceToWallX = Math.Min(x - halfOfRobot, BattleFieldWidth - x - halfOfRobot);
            double distanceToWallY = Math.Min(y - halfOfRobot, BattleFieldHeight - y - halfOfRobot);

            double nextX = x + (Math.Sin(angle) * WALL_STICK);
            double nextY = y + (Math.Cos(angle) * WALL_STICK);
            
            double nextDistanceToWallX = Math.Min(nextX - halfOfRobot, BattleFieldWidth - nextX - halfOfRobot);
            double nextDistanceToWallY = Math.Min(nextY - halfOfRobot, BattleFieldHeight - nextY - halfOfRobot);

            double adjacent = 0;
            int g = 0; // because I'm paranoid about potential infinite loops

            //while (!(testDistanceX > 0  && testDistanceY > 0) && g++ < 25)
            //{
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

                // use your own equivalent of (1 / POSITIVE_INFINITY) instead of 0.005
                // if you want to stay closer to the wall ;)
                // angle += smoothTowardEnemy * orientation * (Math.Abs(Math.Acos(adjacent / WALL_STICK)) + 0.005);
            /*
                nextX = x + (Math.Sin(angle) * WALL_STICK);
                nextY = y + (Math.Cos(angle) * WALL_STICK);
                nextDistanceToWallX = Math.Min(nextX - halfOfRobot, BattleFieldWidth - nextX - halfOfRobot);
                nextDistanceToWallY = Math.Min(nextY - halfOfRobot, BattleFieldHeight - nextY - halfOfRobot);
                */
                if (smoothTowardEnemy == -1)
                {
                    // this method ended with tank smoothing away from enemy... you may
                    // need to note that globally, or maybe you don't care.
                }
            //}

            return angle; // you may want to normalize this
        }


    }
}
