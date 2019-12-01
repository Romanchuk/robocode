using Robocode;
using System.Collections.Generic;
using System.Drawing;

namespace Romanchuk.MoveStrategy
{
    public interface IMoveStrategy
    {
        PointF GetDestination(IEnumerable<Enemy> enemies, Enemy currentTarget, bool forceChangeDirection);
        bool UnsafeMovement { get; }
        void Move(IEnumerable<Enemy> enemies, Enemy currentTarget, bool underAttack);
    }
}
