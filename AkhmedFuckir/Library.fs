namespace Romanchuk
open Robocode

type AkhmedFuckir() =
   inherit AdvancedRobot()

   member val targetEnemyAngle = 0.0 with get, set
   override a.Run() = 
    a.IsAdjustGunForRobotTurn <- true
    a.IsAdjustRadarForRobotTurn <- true
    while true do 
        a.SetTurnGunLeft 45.0
        a.Execute()
   override a.OnScannedRobot(e) = 
        a.targetEnemyAngle <- (e.Bearing + a.GunHeading);
        a.SetFire(1.0)
 