namespace Splerp.Change
{
    interface IHurtable : ICollisionObject
    {
        int MaxHealth { get; }
        int CurrentHealth { get; set; }
        bool IsDead { get; }

        void Damage(int amount);
    }
}
