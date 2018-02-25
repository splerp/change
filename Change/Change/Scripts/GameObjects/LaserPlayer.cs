using Microsoft.Xna.Framework;
using Splerp.Change.Controllers;
using Splerp.Change.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Splerp.Change.GameObjects
{
    public sealed class LaserPlayer : GameObject, ICollisionObject
    {
        public const int DAMAGE_LASER_DIRECT = 100;
        public const int DAMAGE_LASER_INDIRECT = 20;

        public const float LASER_FIRE_SHAKE_AMOUNT = 2f;
        public const int GRAPHIC_OUTER_WIDTH = 2;

        // True for the first frame in which the laser has started to fire.
        public bool firstFrameLaserStart;

        // True for the first frame in which the laser has stopped firing.
        public bool firstFrameLaserEnd;

        private GameController gameController;
        public Shaker laserShake;

        // Set ICollisionObject-related properties.
        public Point Size => new Point(8, 8);
        public Rectangle CollisionRect => new Rectangle(new Point(
            (int)Math.Round(Position.X),
            (int)Math.Round(Position.Y)), Size);

        public Vector2 speed;
        public float thrust = 5f;
        public float friction = 0.9f;

        // How quickly the laser bar depletes / replenishes.
        public float laserChargeReduceBy = 0.02f;
        public float laserChargeIncreaseBy = 0.05f;
        public float laserCharge;

        // True if the laser is currently being fired.
        public bool FiringLaser { get; set; }

        // Position helper properties.
        public Point LeftEyePos => CollisionRect.Location + new Point(2, 2);
        public Point RightEyePos => CollisionRect.Location + new Point(5, 2);
        
        public LaserPlayer(GameController gameControllerIn)
        {
            gameController = gameControllerIn;
            laserShake = new Shaker();

            Reset();
        }

        // Default player's location to the middle of the screen.
        public void Reset()
        {
            SetX((ChangeGame.PLAYABLE_AREA_WIDTH - Size.X) / 2);
            SetY(ChangeGame.PLAYABLE_AREA_HEIGHT / 2 - 20);
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

            // We need extra checks because certain assumptions may
            // no longer be true after the Task.Delay.
            if (gameController.CurrentStage.HasFlag(Stage.StageFlags.LaserPlayerEnabled) && gameController.CurrentState == GameState.Playing && Control.Attack.IsDown)
            {
                SoundController.Play(Sound.LaserLoop, true);
            }
        }

        public void Update(GameTime gameTime)
        {
            var mousePos = InputController.CurrentMousePosition;
            var lineToMouse = (mousePos - CollisionRect.Center);

            Vector2 inputVector = Vector2.Zero;
            if (Control.MoveUp.IsDown)
            {
                var angle = Math.Atan2(lineToMouse.Y, lineToMouse.X);

                if (lineToMouse.ToVector2().LengthSquared() > 1f)
                {
                    inputVector = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * thrust;
                }
            }
            else if (Control.MoveDown.IsDown)
            {
                var angle = Math.Atan2(-lineToMouse.Y, -lineToMouse.X);

                inputVector = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * thrust;
            }

            speed += inputVector * new Vector2((float)gameTime.ElapsedGameTime.TotalSeconds);
            speed *= friction;

            MoveBy(speed);

            RestrictToBounds();

            laserShake.Update();
        }

        public void FixedUpdate()
        {
            var mousePos = InputController.CurrentMousePosition;
            
            var laserButtonDown = Control.Attack.IsDown;

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
            
            if (FiringLaser)
            {
                laserShake.currentAmplitude = LASER_FIRE_SHAKE_AMOUNT;

                // Fire both lasers.
                var laserStartPos1 = LeftEyePos;
                var laserStartPos2 = RightEyePos;
                var lineToMouse1 = mousePos - laserStartPos1;
                var lineToMouse2 = mousePos - laserStartPos2;

                var allHurtables = gameController.enemies
                    .Take(gameController.enemies.Count)
                    .Cast<IHurtable>()
                    .Concat(gameController.notes);

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
            else if (Position.X + Size.X > ChangeGame.PLAYABLE_AREA_WIDTH)
            {
                SetX(ChangeGame.PLAYABLE_AREA_WIDTH - Size.X);
                speed.X *= -1;
            }
            if (Position.Y < 0)
            {
                SetY(0);
                speed.Y *= -1;
            }
            else if (Position.Y + Size.Y > ChangeGame.PLAYABLE_AREA_HEIGHT)
            {
                SetY(ChangeGame.PLAYABLE_AREA_HEIGHT - Size.Y);
                speed.Y *= -1;
            }
        }

        private void PrintPlayerInfo()
        {
            //Console.WriteLine($"PLAYER. pos: {Position.ToPoint().ToString()}; speed: {speed.ToPoint().ToString()}");
        }
    }
}
