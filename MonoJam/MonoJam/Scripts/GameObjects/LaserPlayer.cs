using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoJam.Controllers;
using MonoJam.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MonoJam.GameObjects
{
    public class LaserPlayer : GameObject, ICollisionObject
    {
        public const int DAMAGE_LASER_DIRECT = 100;
        public const int DAMAGE_LASER_INDIRECT = 20;

        public const float LASER_FIRE_SHAKE_AMOUNT = 2f;
        public const int GRAPHIC_OUTER_WIDTH = 2;

        public bool firstFrameLaserStart;
        public bool firstFrameLaserEnd;

        private GameController gc;
        public ShakeController laserShake;

        public Point Size => new Point(8, 8);
        public Rectangle CollisionRect => new Rectangle(new Point(
            (int)Math.Round(Position.X),
            (int)Math.Round(Position.Y)), Size);

        public Vector2 speed;
        public float thrust = 0.1f;
        public float friction = 0.9f;

        public float laserChargeReduceBy = 0.02f;
        public float laserChargeIncreaseBy = 0.05f;
        public float laserCharge;

        public bool FiringLaser { get; set; }

        public Point LeftEyePos => CollisionRect.Location + new Point(2, 2);
        public Point RightEyePos => CollisionRect.Location + new Point(5, 2);
        
        public LaserPlayer(GameController gcIn)
        {
            gc = gcIn;
            laserShake = new ShakeController();

            Reset();
        }

        public void Reset()
        {
            SetX((MonoJam.PLAYABLE_AREA_WIDTH - Size.X) / 2);
            SetY(MonoJam.PLAYABLE_AREA_HEIGHT / 2 - 20);
            speed = Vector2.Zero;

            laserCharge = 1f;
        }

        private async void StartLaser()
        {
            SoundController.Stop(Sound.LaserStart);
            SoundController.Stop(Sound.LaserLoop);
            SoundController.Stop(Sound.LaserEnd);

            SoundController.Play(Sound.LaserStart);

            // Wait for LaserStart sound to play, then start looping.
            await Task.Delay(Sound.LaserStart.data.Duration);

            if (gc.currentState == GameController.GameState.Playing && Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                SoundController.Play(Sound.LaserLoop, true);
            }
        }

        public void Update()
        {
            var kbs = Keyboard.GetState();
            var ms = Mouse.GetState();

            var mousePos = Mouse.GetState().Position / new Point(MonoJam.SCALE) - new Point(0, MonoJam.PLAYABLE_AREA_Y);
            var lineToMouse = (mousePos - CollisionRect.Center);

            laserShake.Update();

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
            
            Vector2 inputVector = Vector2.Zero;
            if (kbs.IsKeyDown(Keys.W))
            {
                var angle = Math.Atan2(lineToMouse.Y, lineToMouse.X);

                if(lineToMouse.ToVector2().LengthSquared() > 1f)
                {
                    inputVector = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * thrust;
                }
            }
            else if (kbs.IsKeyDown(Keys.S))
            {
                var angle = Math.Atan2(-lineToMouse.Y, -lineToMouse.X);

                inputVector = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * thrust;
            }

            speed += inputVector;
            speed *= friction;

            MoveBy(speed);

            RestrictToBounds();
            
            if (FiringLaser)
            {
                laserShake.currentAmplitude = LASER_FIRE_SHAKE_AMOUNT;

                // Fire both lasers.
                var laserStartPos1 = LeftEyePos;
                var laserStartPos2 = RightEyePos;
                var lineToMouse1 = mousePos - laserStartPos1;
                var lineToMouse2 = mousePos - laserStartPos2;

                var allHurtables = gc.enemies
                    .Take(gc.totalEnemies)
                    .Cast<IHurtable>()
                    .Concat(gc.notes);

                foreach(var h in allHurtables)
                {
                    var isHitting = LineCollisionTest.IntersectSegment(h.CollisionRect, laserStartPos1, lineToMouse1)
                        || LineCollisionTest.IntersectSegment(h.CollisionRect, laserStartPos2, lineToMouse2);

                    if (isHitting)
                    {
                        if ((mousePos - h.CollisionRect.Center).ToVector2().Length() < 10f)
                        {
                            h.Damage(DAMAGE_LASER_DIRECT);
                        }
                        else
                        {
                            h.Damage(DAMAGE_LASER_INDIRECT);
                        }
                    }
                }

                if (firstFrameLaserStart)
                {
                    StartLaser();
                }

                firstFrameLaserStart = false;
                firstFrameLaserEnd = true;
            }
            else
            {
                SoundController.Stop(Sound.LaserStart);
                SoundController.Stop(Sound.LaserLoop);

                if (firstFrameLaserEnd)
                {
                    SoundController.Play(Sound.LaserEnd);
                }

                firstFrameLaserStart = true;
                firstFrameLaserEnd = false;
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
