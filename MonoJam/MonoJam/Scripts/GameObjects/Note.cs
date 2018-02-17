using Microsoft.Xna.Framework;
using MonoJam.Controllers;

namespace MonoJam.GameObjects
{
    public class Note : GameObject, ICollisionObject, IHurtable
    {
        public enum NoteType { None, Pink5, Blue10, Red20, Yellow50 }

        public const int WIDTH = 8;
        public const int HEIGHT = 4;

        private GameController gc;

        public Point Size => new Point(WIDTH, HEIGHT);
        public Rectangle CollisionRect => new Rectangle(Position.ToPoint(), Size);

        public bool InRangeForCatching => Position.Y < MonoJam.PLAYABLE_AREA_HEIGHT;
        public bool ReadyToRemove => Position.Y > MonoJam.WINDOW_HEIGHT;
        public bool CaughtByPlayer;

        public VacuumEnemy CaughtByVacuum;
        public bool InsideVacuum;

        public float downSpeed = 0.1f;

        public int invulnCount = 100;
        public int invulnCountdown;

        public int MaxHealth => 1;
        public int CurrentHealth { get; set; }
        public bool IsDead => CurrentHealth <= 0;
        public NoteType Type => NoteType.Pink5;

        public Note(GameController gcIn)
        {
            gc = gcIn;

            SetX(GameController.random.Next(0, MonoJam.WINDOW_WIDTH - WIDTH));
            SetY(-HEIGHT);

            CurrentHealth = MaxHealth;
        }

        public void Update()
        {
            if(CaughtByPlayer)
            {
                if (Position.X < gc.paddlePlayer.CollisionRect.X + PaddlePlayer.GRAPHIC_EDGE_WIDTH)
                {
                    SetX(gc.paddlePlayer.CollisionRect.X + PaddlePlayer.GRAPHIC_EDGE_WIDTH);
                }
                else if (Position.X + WIDTH > gc.paddlePlayer.CollisionRect.Right - PaddlePlayer.GRAPHIC_EDGE_WIDTH)
                {
                    SetX(gc.paddlePlayer.CollisionRect.Right - PaddlePlayer.GRAPHIC_EDGE_WIDTH - WIDTH);
                }

                MoveBy(new Vector2(0, downSpeed));
            }
            else if(CaughtByVacuum != null)
            {
                SetX(CaughtByVacuum.MouthPosX);

                // Move towards the vacuum.
                if (CollisionRect.Y < CaughtByVacuum.CollisionRect.Y)
                {
                    MoveBy(new Vector2(0, 1));
                }
                else if (CollisionRect.Y > CaughtByVacuum.CollisionRect.Y + VacuumEnemy.HEIGHT)
                {
                    MoveBy(new Vector2(0, -1));
                }
                else
                {
                    //Caught.
                    InsideVacuum = true;
                }

                invulnCountdown = invulnCount;
            }
            else
            {
                MoveBy(new Vector2(0, downSpeed));
            }

            invulnCountdown--;
        }

        public void Damage(int amount)
        {
            if(!InsideVacuum && invulnCountdown <= 0)
            {
                CurrentHealth -= amount;
            }
        }
    }
}
