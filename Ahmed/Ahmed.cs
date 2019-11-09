using Robocode;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using Romanchuk.BattleStrategy;

namespace Romanchuk
{
    public class Ahmed : AdvancedRobot
    {
        private readonly IDictionary<string, ScannedRobotEvent> enemies = new Dictionary<string, ScannedRobotEvent>();

        private readonly IBattleStrategy BattleStrategy;

        private long lastTimeBeingHit = -1;

        public Ahmed()
        {
            BattleStrategy = new RageBattleStrategy<Ahmed>(this); 
        }

        public override void Run() {
            IsAdjustGunForRobotTurn = true;
            IsAdjustRadarForRobotTurn = true;
            BattleStrategy.Target.Reset();

            int colorIteration = 1;
            SetAllColors(Color.Black);

            while (true) {


                SetAllColors(Color.Black);
                Out.WriteLine($"----------------------------");
                SetTurnRadarRight(Rules.RADAR_TURN_RATE);





                /*
                if (RageTarget == null || Energy <= 10) {
                */

                // SetTurnRight(turnRadians);
                /*
                } else {
                    Out.WriteLine($"Enemy ({RageTarget.Name})");
                    ChangeColor(ref colorIteration);
                    // SetTurnRightRadians(RageTarget.BearingRadians);
                    var turnGunRadians = GetDiffTargetAndGunRadians(RageTarget.BearingRadians, true);
                    SetTurnGunRightRadians(turnGunRadians);
                    if (Math.Abs(turnGunRadians) < 0.15)
                    {
                        SetAdjustedFire(RageTarget.Energy, RageTarget.Distance);
                    }
                /*
            }


            */

                BattleStrategy.ChangeColor(ref colorIteration);
                BattleStrategy.Move();
                BattleStrategy.Shoot();

                Execute();
            }
        }


        public override void OnScannedRobot(ScannedRobotEvent ev)
        {
            var oldEvents = enemies
                .Where(e => Time - e.Value.Time > 12)
                .ToList();
            foreach (var oe in oldEvents)
            {
                enemies.Remove(oe);
            }
            enemies[ev.Name] = ev;

            BattleStrategy.ChooseTarget(
                enemies
                    .Where(e => e.Value.Energy >= 0)
                    .Take(Others)
                    .Select(e => e.Value)
            );
        }

        public override void OnRobotDeath(RobotDeathEvent e)
        {
            enemies.Remove(e.Name);
            if (e.Name.Equals(BattleStrategy.Target.Name))
            {
                BattleStrategy.Target.Reset();
            }
        }

        public override void OnHitByBullet(HitByBulletEvent e)
        {
            lastTimeBeingHit = e.Time;
        }

    }
}
