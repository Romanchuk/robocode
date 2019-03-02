namespace Romanchuk

module public Helper = 
    open Robocode
    open System.Drawing
    let public WALL_STICK = 160.0f
    
    let project (sourceLocation: PointF, angle: float32, length: float32) =
        new PointF (sourceLocation.X + sin(angle) * length, sourceLocation.Y + cos(angle) * length)
    (*
    let wallSmoothing (botLocation: PointF, angle: float32, orientation: float32) =
        let _fieldRect = new RectangleF(18.0f, 18.0f, 764.0f, 564.0f)
        seq {
            while not (_fieldRect.Contains (project (botLocation, angle, WALL_STICK))) do 
                yield orientation * 0.05f
        } |> Seq.sum
    *)
    let bulletVelocity power = (20.0 - (3.0*power)) 
    let maxEscapeAngle velocity = asin 8.0/velocity
    let absoluteBearing (source: PointF, target: PointF) =
        atan2 (target.X - source.X) (target.Y - source.Y)

    let limit (min: double, value: double, max: double) =
        System.Math.Max (min, System.Math.Min (value, max))

    


    let setBackAsFront (robot: AdvancedRobot, goAngle: float) =
        let angle = goAngle
            // Utils.normalRelativeAngle (goAngle - robot.getHeadingRadians);
        if  abs angle > System.Math.PI/(float 2) then
            if angle < 0.0 then
                robot.SetTurnRightRadians (System.Math.PI + angle);
            else
                robot.SetTurnLeftRadians (System.Math.PI - angle)
            
            robot.SetBack (float 100)
        else
            if angle < 0.0 then
                robot.SetTurnLeftRadians (-1.0*angle)
            else
                robot.SetTurnRightRadians angle
            robot.SetAhead (float 100)





    
open Robocode
open System.Drawing
open Helper
type AhmedFuckir() =

    inherit AdvancedRobot()

    member val targetEnemyAngle = 0.0 with get, set
    override a.Run() = 
        a.SetAllColors(Color.Crimson)
        a.IsAdjustGunForRobotTurn <- true
        a.IsAdjustRadarForRobotTurn <- true
    
        while true do 
            a.SetTurnGunLeft 45.0
            a.Execute()
    override a.OnScannedRobot(e) = 
        a.targetEnemyAngle <- (e.Bearing + a.GunHeading);
        a.SetFire(1.0)
 