using Robocode;
using System.Collections.Generic;
using System.Drawing;

namespace Romanchuk.MoveStrategy
{
    public interface IMoveStrategy
    {
        PointF GetDestination(IEnumerable<Enemy> enemies, bool forceChangeDirection);
        bool UnsafeMovement { get; }
    }
}
