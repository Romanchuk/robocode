using Robocode;
using System.Drawing;
using System;



namespace Romanchuk
{
    public class Ahmed : AdvancedRobot
    {
        override public void Run() {
            IsAdjustGunForRobotTurn = true;
            IsAdjustRadarForRobotTurn = true;

            int colorIteration = 1;
            ChangeColor(ref colorIteration);

            while (true) {
                ChangeColor(ref colorIteration);
                SetFire(1.0);
                SetTurnRadarRight(180.0);
                Execute();
            }
        }


        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            var lateralVelocity = Velocity * (Math.Sin(e.BearingRadians));
            var absBearing = e.BearingRadians + HeadingRadians;
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
