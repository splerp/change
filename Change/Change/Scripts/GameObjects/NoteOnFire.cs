using Microsoft.Xna.Framework;
using Splerp.Change.Controllers;

namespace Splerp.Change.GameObjects
{
    public sealed class NoteOnFire : GameObject
    {
        public const int HEIGHT = 16;
        public const int TOTAL_FRAMES = 6;

        // How fast the burning note moves up.
        public float upSpeed = -20f;

        // How quickly the fire animation should play.
        private float animationSpeed = 0.1f;
        private float animationCount;
        public int animationFrame = 0;

        // Property to determine if it's safe to remove this object.
        public bool ReadyToRemove => animationFrame >= TOTAL_FRAMES;

        public Note.NoteType Type { get; private set; }
        
        public bool IsFlipped { get; set; }

        public NoteOnFire(Note note)
        {
            SetX(note.CollisionRect.X);
            SetY(note.CollisionRect.Y);

            Type = note.Type;

            IsFlipped = GameController.random.Next(0, 2) == 0;

            animationCount = animationSpeed;
        }

        public void Update(GameTime gameTime)
        {
            MoveBy(new Vector2(0, upSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds));

            // Update selected animation frame.
            animationCount -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (animationCount <= 0)
            {
                animationCount += animationSpeed;
                animationFrame++;
            }
        }
    }
}
