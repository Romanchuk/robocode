using Robocode;
using System.Collections.Generic;

namespace Romanchuk.BattleStrategy
{
    public interface IBattleStrategy
    {
        void Shoot();

        void ChangeColor(ref int colorIteration);

        void Move();

        void ChooseTarget(IEnumerable<ScannedRobotEvent> enemies);

        Target Target { get; }
    }
}
