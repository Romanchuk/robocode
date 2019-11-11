using Robocode;
using System.Drawing;

namespace Romanchuk.MoveStrategy
{
    public interface IMoveStrategy
    {
        PointF SetDestination(Enemy[] enemies, Robot myRobot);
    }
}
