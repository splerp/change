using Microsoft.Xna.Framework;

namespace Splerp.Change
{
    interface ICollisionObject
    {
        Point Size { get; }
        Rectangle CollisionRect { get; }
    }
}
