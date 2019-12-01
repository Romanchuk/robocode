using System.Collections.Generic;
using Robocode;

namespace Romanchuk.BattleStrategy
{
    public interface IBattleStrategy
    {
        void Init();
        void Shoot();

        void ChangeColor(ref int colorIteration);

        void Move();

        void ChooseTarget(IEnumerable<Enemy> enemies);

        void ResetTarget();

        Enemy CurrentTarget { get; }

        void AttachHitByBulletEvents(IEnumerable<HitByBulletEvent> bulletHits);
    }
}
