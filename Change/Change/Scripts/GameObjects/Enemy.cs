using Microsoft.Xna.Framework;
using MonoJam.Controllers;
using System;

namespace MonoJam.GameObjects
{
    public abstract class Enemy : GameObject, ICollisionObject, IHurtable
    {
        public abstract int CoinsOnDeath { get; }
        public abstract Point Size { get; }
        public Rectangle CollisionRect => new Rectangle(Position.ToPoint(), Size);

        public bool ReadyToRemove => Position.X + Size.X < 0 || Position.X > MonoJam.PLAYABLE_AREA_WIDTH;

        // TODO probably don't want in base Enemy class.
        public float thrust = 0.5f;
        public int direction;
        public float yOffset;
        public float yOffsetDiff;

        public abstract int MaxHealth { get; }
        public int CurrentHealth { get; set; }
        public bool IsDead => CurrentHealth <= 0;

        public void Init()
        {
            CurrentHealth = MaxHealth;
        }

        public abstract void OnDeath();
        public abstract void Update();

        public void Damage(int amount)
        {
            CurrentHealth -= amount;
        }
    }
}
