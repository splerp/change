using Microsoft.Xna.Framework;
using MonoJam.Controllers;

namespace MonoJam.GameObjects
{
    public class NoteOnFire : GameObject
    {
        public float upSpeed = -0.33f;

        private float animationSpeed = 6;
        private float animationCount;
        public int animationFrame = 0;

        public const int HEIGHT = 16;
        public const int TOTAL_FRAMES = 6;

        public bool ReadyToRemove => animationFrame >= TOTAL_FRAMES;

        public Note.NoteType Type { get; private set; }

        // Shouldn't be here? (it's a graphics thing)
        public bool IsFlipped { get; set; }

        public NoteOnFire(Note note)
        {
            SetX(note.CollisionRect.X);
            SetY(note.CollisionRect.Y);

            Type = note.Type;

            IsFlipped = GameController.random.Next(0, 2) == 0;

            animationCount = animationSpeed;
        }

        public void Update()
        {
            MoveBy(new Vector2(0, upSpeed));

            // Update selected animation frame.
            // TODO: Based on time passed
            animationCount--;
            if (animationCount <= 0)
            {
                animationCount += animationSpeed;
                animationFrame++;
            }
        }
    }
}
