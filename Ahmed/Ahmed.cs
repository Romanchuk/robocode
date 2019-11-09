using Robocode;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using Romanchuk.BattleStrategy;

namespace Romanchuk
{
    public class Ahmed : AdvancedRobot
    {
        private readonly IDictionary<string, ScannedRobotEvent> _enemies = new Dictionary<string, ScannedRobotEvent>();

        private readonly IBattleStrategy _battleStrategy;

        private long lastTimeBeingHit = -1;

        public Ahmed()
        {
            _battleStrategy = new RageBattleStrategy<Ahmed>(this); 
        }

        public override void Run() {
            IsAdjustGunForRobotTurn = true;
            IsAdjustRadarForRobotTurn = true;
            _battleStrategy.Target.Reset();

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

                _battleStrategy.ChangeColor(ref colorIteration);
                _battleStrategy.Move();
                _battleStrategy.Shoot();

                Execute();
            }
        }


        public override void OnScannedRobot(ScannedRobotEvent ev)
        {
            var oldEvents = _enemies
                .Where(e => Time - e.Value.Time > 12)
                .ToList();
            foreach (var oe in oldEvents)
            {
                _enemies.Remove(oe);
            }
            _enemies[ev.Name] = ev;

            _battleStrategy.ChooseTarget(
                _enemies
                    .Where(e => e.Value.Energy >= 0)
                    .Take(Others)
                    .Select(e => e.Value)
            );
        }

        public override void OnRobotDeath(RobotDeathEvent e)
        {
            _enemies.Remove(e.Name);
            if (e.Name.Equals(_battleStrategy.Target.Name))
            {
                _battleStrategy.Target.Reset();
            }
        }

        public override void OnHitByBullet(HitByBulletEvent e)
        {
            lastTimeBeingHit = e.Time;
        }

    }
}
