using Microsoft.Xna.Framework;
using MonoJam.Controllers;
using System;

namespace MonoJam.GameObjects
{
    public class VacuumEnemy : Enemy
    {
        public const int WIDTH = 12;
        public const int HEIGHT = 11;
        public const int WIDTH_HEAD = 11;
        public const int MAX_NOTES_HELD = 5;

        public override Point Size => new Point(WIDTH + WIDTH_HEAD, HEIGHT);
        public override int MaxHealth => 1000;
        public override int CoinsOnDeath => 0;
        public int MouthPosX => CollisionRect.X + (direction > 0 ? WIDTH + 2 : 1);
        public int CentreBodyX => CollisionRect.X + (direction > 0 ? 0 : WIDTH_HEAD);

        public readonly Note[] NotesHeld = new Note[MAX_NOTES_HELD];

        public float yOffsetCount;
        public float yPos;

        public float sinAmp = 0.5f;
        public float sinPer = 0.03f;

        public int totalNotesHeld;
        public bool lookingUp;

        public VacuumEnemy()
        {
            yOffsetCount = GameController.random.Next(1, 1000);

            thrust = 0.2f;
            direction = GameController.random.Next(1, 3) == 1 ? 1 : -1;
            thrust *= direction;

            int halfAreaSize = (MonoJam.PLAYABLE_AREA_WIDTH + Size.X) / 2;
            SetX(MonoJam.PLAYABLE_AREA_WIDTH - (halfAreaSize * direction + halfAreaSize));

            for(int i = 0; i < MAX_NOTES_HELD; i++)
            {
                NotesHeld[i] = null;
            }

            yPos = GameController.random.Next((int)sinAmp, MonoJam.PLAYABLE_AREA_HEIGHT - Size.Y - (int)sinAmp / 2);
        }

        public override void Update()
        {
            var previousYOffset = yOffset;

            // TODO: Based on time passed.
            yOffsetCount++;

            yOffset = sinAmp * (float)Math.Sin(yOffsetCount * sinPer);
            yOffsetDiff = yOffset - previousYOffset;

            MoveBy(new Vector2(thrust, 0));
            SetY(yPos + yOffset);
        }

        public void TryCollect(Note note)
        {
            if(note.IsDead || note.CaughtByPlayer || note.CaughtByVacuum != null)
            {
                return;
            }

            if (Math.Abs(note.CollisionRect.X - MouthPosX) > 1)
            {
                return;
            }

            if (totalNotesHeld >= MAX_NOTES_HELD)
            {
                return;
            }

            CollectNote(note);
        }

        public void CollectNote(Note note)
        {
            if(totalNotesHeld < MAX_NOTES_HELD)
            {
                note.CaughtByVacuum = this;
                NotesHeld[totalNotesHeld++] = note;

                lookingUp = note.CollisionRect.Y < CollisionRect.Y;

                SoundController.Play(Sound.Slurp);
            }
        }

        public override void OnDeath()
        {
            ReleaseNotes();

            SoundController.Play(Sound.Boom);
        }

        public void ReleaseNotes()
        {
            for(int i = totalNotesHeld - 1; i >= 0; i--)
            {
                if(NotesHeld[i].InsideVacuum)
                {
                    NotesHeld[i].SetX(GameController.random.Next(1, 5) - 2 + CentreBodyX);
                }

                NotesHeld[i].CaughtByVacuum = null;
                NotesHeld[i].InsideVacuum = false;
                
                NotesHeld[i] = null;
            }
        }
    }
}
