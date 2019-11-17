using Robocode;
using System.Collections.Generic;
using System.Drawing;

namespace Romanchuk.MoveStrategy
{
    public interface IMoveStrategy
    {
        PointF SetDestination(IEnumerable<Enemy> enemies);
        bool UnsafeMovement { get; }
    }
}
