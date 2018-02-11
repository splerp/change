using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace MonoJam
{
    public class Player : GameObject, ICollisionObject
    {
        public Point Size => new Point(10, 10);
        public Rectangle CollisionRect => new Rectangle(Position.ToPoint(), Size);

        public Vector2 speed;
        public float thrust = 1f;
        public float friction = 0.9f;
        
        public void Update()
        {
            var kbs = Keyboard.GetState();

            var inputVector = new Vector2(
                kbs.IsKeyDown(Keys.A) ? -1 : kbs.IsKeyDown(Keys.D) ? 1 : 0,
                kbs.IsKeyDown(Keys.W) ? -1 : kbs.IsKeyDown(Keys.S) ? 1 : 0) * thrust;

            speed += inputVector;
            speed *= friction;

            MoveBy(speed);

            PrintPlayerInfo();
        }

        private void PrintPlayerInfo()
        {
            Console.WriteLine($"PLAYER. pos: {Position.ToPoint().ToString()}; speed: {speed.ToPoint().ToString()}");
        }
    }
}
