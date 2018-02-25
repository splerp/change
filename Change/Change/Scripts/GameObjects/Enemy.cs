using Microsoft.Xna.Framework;

namespace Splerp.Change.GameObjects
{
    public abstract class Enemy : GameObject, ICollisionObject, IHurtable
    {
        public enum HorizontalDirection { Left = -1, Right = 1 }

        public abstract int CoinsOnDeath { get; }
        public abstract Point Size { get; }
        public Rectangle CollisionRect => new Rectangle(Position.ToPoint(), Size);

        public bool ReadyToRemove => Position.X + Size.X < 0 || Position.X > ChangeGame.PLAYABLE_AREA_WIDTH;

        public Vector2 Offset { get; set; }
        public Vector2 PreviousOffset { get; set; }
        public Vector2 Speed { get; set; }

        public HorizontalDirection Direction { get; set; }

        public abstract int MaxHealth { get; }
        public int CurrentHealth { get; set; }
        public bool IsDead => CurrentHealth <= 0;

        public void Init()
        {
            CurrentHealth = MaxHealth;
        }

        public abstract void OnDeath();
        public abstract void Update(GameTime gameTime);

        public void Damage(int amount)
        {
            CurrentHealth -= amount;
        }
    }
}
