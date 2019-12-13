using Robocode;
using System.Linq;
using System.Collections.Generic;
using Romanchuk.BattleStrategy;
namespace Romanchuk
{
    public class Ahmed : AdvancedRobot
    {
        private IDictionary<string, Enemy> _enemies = new Dictionary<string, Enemy>();

        private readonly IBattleStrategy _battleStrategy;

        private List<HitByBulletEvent> _bulletHitEvents;
        private List<HitRobotEvent> _hitRobotEvents;

        public Ahmed()
        {
            _battleStrategy = new BattleStrategy<Ahmed>(this);
        }

        public override void Run()
        {
            IsAdjustGunForRobotTurn = true;
            IsAdjustRadarForRobotTurn = true;

            _enemies = new Dictionary<string, Enemy>();
            _bulletHitEvents = new List<HitByBulletEvent>();
            _hitRobotEvents = new List<HitRobotEvent>();
            _battleStrategy.Init();
            _battleStrategy.AttachHitByBulletEvents(_bulletHitEvents);
            _battleStrategy.ResetTarget();


            int colorIteration = 1;
            _battleStrategy.ChangeColor(ref colorIteration);

            while (true)
            {
                Out.WriteLine($"----------------------------");
                _battleStrategy.ChangeColor(ref colorIteration);

                _battleStrategy.ActualEnemies();
                _battleStrategy.Move();

                _battleStrategy.Shoot();

                Execute();
            }
        }


        public override void OnScannedRobot(ScannedRobotEvent ev)
        {
            var oldEvents = _enemies
                .Where(e => Time - e.Value.Instance.Time > 12)
                .ToList();
            foreach (var oe in oldEvents)
            {
                _enemies.Remove(oe);
            }

            if (!_enemies.Keys.Contains(ev.Name))
            {
                _enemies[ev.Name] = new Enemy(this);
            }

            _enemies[ev.Name].Update(ev);

            _battleStrategy.ChooseTarget(
                _enemies
                    .Take(Others)
                    .Select(e => e.Value),
                _hitRobotEvents
            );
        }

        public override void OnRobotDeath(RobotDeathEvent e)
        {
            _enemies.Remove(e.Name);
            if (_battleStrategy.CurrentTarget != null)
            {
                if (e.Name.Equals(_battleStrategy.CurrentTarget?.Name))
                {
                    _battleStrategy.ResetTarget();
                }
            }
        }

        public override void OnHitByBullet(HitByBulletEvent e)
        {
            _bulletHitEvents.Add(e);
        }

        public override void OnHitRobot(HitRobotEvent e)
        {
            _hitRobotEvents.Add(e);
        }

    }
}
