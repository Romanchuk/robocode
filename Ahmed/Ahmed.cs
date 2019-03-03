using Robocode;
using Robocode.Util;
using System.Drawing;
using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;


namespace Romanchuk
{
    public class Ahmed : AdvancedRobot
    {
        private readonly IDictionary<string, ScannedRobotEvent> enemies = new Dictionary<string, ScannedRobotEvent>();;

        override public void Run() {
            IsAdjustGunForRobotTurn = true;
            IsAdjustRadarForRobotTurn = true;

            int colorIteration = 1;
            ChangeColor(ref colorIteration);

            while (true) {
                ChangeColor(ref colorIteration);
                SetTurnRadarRight(180.0);

                var target = enemies
                    .Where(e => e.Value.Energy > 0)
                    .Where(e => e.Value.Time < Time - 2) // ignore 3 turn old enemies detections, they must be already dead
                    .Select(d => d.Value)
                    .OrderBy(e => e.Energy)
                    .ThenBy(e => e.Distance)
                    .FirstOrDefault();
                if (target != null)
                {
                    var normAbsBearing = HeadingRadians + target.BearingRadians;                    
                    Debug.WriteLine($"Enemy ({target.Name}) Abs Bearing Norm: " + normAbsBearing);
                    Debug.WriteLine("Gun Heading: " + GunHeadingRadians);
                    var b = Utils.NormalRelativeAngle(normAbsBearing - GunHeadingRadians);
                    Debug.WriteLine("Radians: " + b);
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

               

                Execute();
            }
        }


        override public void OnScannedRobot(ScannedRobotEvent e)
        {
            var lateralVelocity = Velocity * (Math.Sin(e.BearingRadians));
            var absBearing = e.BearingRadians + HeadingRadians;
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


    }
}
