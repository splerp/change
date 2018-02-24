using Microsoft.Xna.Framework;

namespace MonoJam
{
    interface ICollisionObject
    {
        Point Size { get; }
        Rectangle CollisionRect { get; }
    }
}
