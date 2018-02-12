using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace MonoJam
{
    public class Player : GameObject, ICollisionObject
    {
        public const int DAMAGE_LASER_DIRECT = 100;
        public const int DAMAGE_LASER_INDIRECT = 20;

        private GameController gc;

        public Point Size => new Point(8, 8);
        public Rectangle CollisionRect => new Rectangle(new Point(
            (int)Math.Round(Position.X),
            (int)Math.Round(Position.Y)), Size);

        public Vector2 speed;
        public float thrust = 0.1f;
        public float friction = 0.9f;

        public float laserChargeReduceBy = 0.02f;
        public float laserChargeIncreaseBy = 0.05f;
        public float laserCharge = 1f;

        public bool FiringLaser { get; set; }
        
        public Player(GameController gcIn)
        {
            gc = gcIn;
        }

        public void Update()
        {
            var kbs = Keyboard.GetState();
            var ms = Mouse.GetState();

            var laserButtonDown = ms.LeftButton == ButtonState.Pressed;

            if(laserButtonDown)
            {
                laserCharge -= laserChargeReduceBy;
                if(laserCharge < 0)
                {
                    laserCharge = 0;
                }
            }
            else
            {
                laserCharge += laserChargeIncreaseBy;
                if (laserCharge > 1)
                {
                    laserCharge = 1;
                }
            }

            FiringLaser = laserButtonDown && laserCharge > 0;

            var inputVector = new Vector2(
                kbs.IsKeyDown(Keys.A) ? -1 : kbs.IsKeyDown(Keys.D) ? 1 : 0,
                kbs.IsKeyDown(Keys.W) ? -1 : kbs.IsKeyDown(Keys.S) ? 1 : 0) * thrust;

            speed += inputVector;
            speed *= friction;

            MoveBy(speed);

            RestrictToBounds();

            // Damage enemies in line.
            if(FiringLaser)
            {
                for (int i = 0; i < gc.totalEnemies; i++)
                {
                    var enemy = gc.enemies[i];

                    var mousePos = Mouse.GetState().Position / new Point(MonoJam.SCALE) - new Point(0, MonoJam.PLAYABLE_AREA_Y);
                    var lineToMouse = mousePos - Position.ToPoint();

                    var hittingEnemy = LineCollisionTest.IntersectSegment(enemy.CollisionRect, Position.ToPoint(), lineToMouse);

                    if (hittingEnemy)
                    {
                        if ((mousePos - enemy.CollisionRect.Center).ToVector2().Length() < 5f)
                        {
                            enemy.Damage(DAMAGE_LASER_DIRECT);
                        }
                        else
                        {
                            enemy.Damage(DAMAGE_LASER_INDIRECT);
                        }
                    }
                }
            }

            PrintPlayerInfo();
        }

        private void RestrictToBounds()
        {
            if (Position.X < 0)
            {
                SetX(0);
                speed.X *= -1;
            }
            else if (Position.X + Size.X > MonoJam.PLAYABLE_AREA_WIDTH)
            {
                SetX(MonoJam.PLAYABLE_AREA_WIDTH - Size.X);
                speed.X *= -1;
            }
            if (Position.Y < 0)
            {
                SetY(0);
                speed.Y *= -1;
            }
            else if (Position.Y + Size.Y > MonoJam.PLAYABLE_AREA_HEIGHT)
            {
                SetY(MonoJam.PLAYABLE_AREA_HEIGHT - Size.Y);
                speed.Y *= -1;
            }
        }

        private void PrintPlayerInfo()
        {
            Console.WriteLine($"PLAYER. pos: {Position.ToPoint().ToString()}; speed: {speed.ToPoint().ToString()}");
        }
    }
}
