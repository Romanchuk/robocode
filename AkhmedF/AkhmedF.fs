namespace Romanchuk
open Robocode

// module AkhmedF =
//    let hello name =
///        printfn "Hello %s" name
type AkhmedF() =
   inherit Robot()
   override a.Run() = while true do a.TurnLeft(45.0);
   override a.OnScannedRobot(e) = a.Fire(1.0)
 
