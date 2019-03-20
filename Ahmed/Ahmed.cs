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
    enum Strategy : byte
    {
        None = 0,
        Deathmatch = 1,
        Versus = 2,
        Rage = 4
    }


    // [SecuritySafeCritical]
    //[System.Security.SecurityCritical()]
    public class Ahmed : AdvancedRobot
    {
        private readonly IDictionary<string, ScannedRobotEvent> enemies = new Dictionary<string, ScannedRobotEvent>();

        private byte CurrentStrategy = (byte) Strategy.None;
        private ScannedRobotEvent CurrentTarget = null;
        private ScannedRobotEvent RageTarget = null;
        private BehaviorSubject<int> Observable;

        /*static Ahmed()
        {
            Debug.WriteLine("Static ctor");
            // AppDomain.CurrentDomain.Load(resources.System_Reactive);
            String resourceName = "Romanchuk.Resources.System.Reactive.dll";
            
            foreach (var a in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                Debug.WriteLine(a);
            }
            
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    Debug.WriteLine($"Could not load resource \"{resourceName}\"");
                    return;
                }
                Byte[] assemblyData = new Byte[stream.Length];
                stream.Read(assemblyData, 0, assemblyData.Length);
                Assembly.Load(assemblyData);
                Debug.WriteLine($"Resource \"{resourceName}\" loaded");
            }*/
        /*/*
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            String resourceName = "System_Reactive" +
               new AssemblyName(args.Name).Name + ".dll";
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                Byte[] assemblyData = new Byte[stream.Length];
                stream.Read(assemblyData, 0, assemblyData.Length);
                return Assembly.Load(assemblyData);
            }
        };

    }*/
        // [SecuritySafeCritical]
        //[System.Security.SecurityCritical]
        override public void Run() {
            IsAdjustGunForRobotTurn = true;
            IsAdjustRadarForRobotTurn = true;

            
            int colorIteration = 1;
            ChangeColor(ref colorIteration);


            Observable = new BehaviorSubject<int>(1);

            while (true) {
                Observable.OnNext(3);
                Out.WriteLine($"----------------------------");
                CurrentStrategy = Others == 1 ? (byte)Strategy.Versus : (byte)Strategy.Deathmatch;
                
                SetTurnRadarRight(Rules.RADAR_TURN_RATE);

                var turnRadians = Utils.NormalAbsoluteAngle(move(X, Y, HeadingRadians, 1, 1));
                var nextHeading = HeadingRadians + turnRadians;
 

                // ScannedSubject.AsObservable().
                var lifeEnemies = enemies
                    .Where(e => e.Value.Energy > 0)
                    .Select(d => d.Value)
                    .OrderByDescending(e => e.Energy)
                    .ThenByDescending(e => e.Distance)
                    .Take(Others);
                RageTarget = lifeEnemies.FirstOrDefault(e => e.Name.Equals(RageTarget?.Name))
                        ?? lifeEnemies.Where(e => e.Distance < 500).FirstOrDefault(e => e.Energy < 40);
                
                if (RageTarget == null) {

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
                        SetTurnGunRightRadians(GetGunTurnRightRadians(target.BearingRadians));
                   
                        SetFire(2.0);
                    }

                    SetAhead(4.0);
                    SetTurnRight(turnRadians);
                } else {
                    Out.WriteLine($"Enemy ({RageTarget.Name})");
                    ChangeColor(ref colorIteration);
                    SetTurnRightRadians(RageTarget.BearingRadians);
                    var turnGunRadians = GetGunTurnRightRadians(RageTarget.BearingRadians);
                    SetTurnGunRightRadians(turnGunRadians);
                    if (Math.Abs(turnGunRadians) < 0.3)
                    {
                        SetFire(1.0);
                    }
                    if (Math.Abs(RageTarget.BearingRadians) < 1.0)
                    {
                        SetAhead(Rules.MAX_VELOCITY);
                    }
                    else
                        SetAhead(Rules.MAX_VELOCITY / 2);
                    
                }

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
            if (RageTarget == null)
                return;
            if (RageTarget.Name.Equals(e.Name) && e.Energy < 0)
            {
                RageTarget = null;
                return;
            }
            SetTurnGunRightRadians(GetGunTurnRightRadians(e.BearingRadians));

            if (this.GunHeat == 0)
            {
                if (e.Energy > 16)
                {
                    SetFire(3);
                }
                else if (e.Energy > 10)
                {
                    SetFire(2);
                    // timesShootBullet++;
                }
                else if (e.Energy > 4)
                {
                    SetFire(1);
                    // timesShootBullet++;
                }
                else if (e.Energy > 2)
                {
                    SetFire(.5);
                    // timesShootBullet++;
                }
                else if (e.Energy > .4)
                {
                    SetFire(.1);
                    // timesShootBullet++;
                }
            }
            // ahead(40); // Ram him again!
        }

       
        public override void OnHitByBullet(HitByBulletEvent e)
        {
            
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

        private double GetGunTurnRightRadians(double targetBearingRadians)
        {
            var normAbsBearing = HeadingRadians + targetBearingRadians;
            return Utils.NormalRelativeAngle(normAbsBearing - GunHeadingRadians);
        }


    }
}
