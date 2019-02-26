using Robocode;

namespace Romanchuk
{
    public class Akhmed : Robot
    {
        public override void Run()
        {
            // Perform your initialization for your robot here

            while (true)
            {
                this.TurnLeft(45);
                // Perform robot logic here calling robot commands etc.
            }
        }

        // Robot event handler, when the robot sees another robot
        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            // We fire the gun with bullet power = 1
            Fire(1);
        }
    }
}
