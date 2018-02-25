using Microsoft.Xna.Framework;
using Splerp.Change.Controllers;
using System;

namespace Splerp.Change.GameObjects
{
    public sealed class VacuumEnemy : Enemy
    {
        public const int WIDTH = 12;
        public const int HEIGHT = 11;
        public const int WIDTH_HEAD = 11;
        public const int MAX_NOTES_HELD = 5;

        // Set Enemy-related properties.
        public override Point Size => new Point(WIDTH + WIDTH_HEAD, HEIGHT);
        public override int MaxHealth => 3500;
        public override int CoinsOnDeath => 125;

        // Position variables that depend on the direction the vacuum is travelling.
        public int MouthPosX => CollisionRect.X + (Direction > 0 ? WIDTH + 2 : 1);
        public int CentreBodyX => CollisionRect.X + (Direction > 0 ? 0 : WIDTH_HEAD);

        public readonly Note[] NotesHeld = new Note[MAX_NOTES_HELD];

        // Used for Y offset calculation.
        public double sinAmp = 0.5d;
        public double sinPer = 2d;

        public double yOffsetCount;
        public float yPos;

        // How many notes the vacuum is currently "holding".
        // This includes notes in the process of being sucked up.
        public int totalNotesHeld;

        // True is looking up, false if looking down..
        public bool lookingUp;

        public VacuumEnemy()
        {
            yOffsetCount = GameController.random.Next(1, 1000);

            Speed = new Vector2(11f, 0);
            Direction = GameController.random.Next(0, 2) == 0 ? HorizontalDirection.Left : HorizontalDirection.Right;
            Speed *= (int)Direction;

            int halfAreaSize = (ChangeGame.PLAYABLE_AREA_WIDTH + Size.X) / 2;
            SetX(ChangeGame.PLAYABLE_AREA_WIDTH - (halfAreaSize * (int)Direction + halfAreaSize));

            for(int i = 0; i < MAX_NOTES_HELD; i++)
            {
                NotesHeld[i] = null;
            }

            yPos = GameController.random.Next((int)sinAmp, ChangeGame.PLAYABLE_AREA_HEIGHT - Size.Y - (int)sinAmp / 2);
        }

        public override void Update(GameTime gameTime)
        {
            PreviousOffset = Offset;
            
            yOffsetCount += gameTime.ElapsedGameTime.TotalSeconds;

            Offset = new Vector2(0, (float)(sinAmp * Math.Sin(yOffsetCount * sinPer)));

            MoveBy(Speed * (float)gameTime.ElapsedGameTime.TotalSeconds);
            SetY(yPos + Offset.Y);
        }

        // Run checks before successfully collecting the note.
        public void TryCollect(Note note)
        {
            // If dead, can't collect.
            if (note.IsDead)
            {
                return;
            }

            // If being held by something, can't collect.
            if ( note.CaughtByPlayer || note.CaughtByVacuum != null)
            {
                return;
            }

            // If too far away in X direction, can't collect.
            if (Math.Abs(note.CollisionRect.X - MouthPosX) > 1)
            {
                return;
            }

            // If holding max number of notes, can't collect.
            if (totalNotesHeld >= MAX_NOTES_HELD)
            {
                return;
            }

            CollectNote(note);
        }

        public void CollectNote(Note note)
        {
            // Set references.
            note.CaughtByVacuum = this;
            NotesHeld[totalNotesHeld++] = note;

            // Look towards note.
            lookingUp = note.CollisionRect.Y < CollisionRect.Y;

            SoundController.Play(Sound.Slurp);
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
                // If note had been fully collected, give it a slightly randomised X position.
                if(NotesHeld[i].InsideVacuum)
                {
                    NotesHeld[i].SetX(GameController.random.Next(1, 5) - 2 + CentreBodyX);
                }

                NotesHeld[i].CaughtByVacuum = null;
                NotesHeld[i].InsideVacuum = false;
                
                NotesHeld[i] = null;
            }
        }

        // A note could be destroyed by the player after it has
        // been "collected" (while it is still moving towards the vacuum).
        // We need to be able to handle a single note being removed
        // from the vacuum.
        public void RemoveNoteFromVacuum(Note note)
        {
            bool found = false;

            for(int i = 0; i < totalNotesHeld; i++)
            {
                if(NotesHeld[i] == note)
                {
                    NotesHeld[i].CaughtByVacuum = null;
                    NotesHeld[i].InsideVacuum = false;

                    found = true;
                }

                // If note was found, remove from array and move remaining notes along.
                if(found)
                {
                    if(i < totalNotesHeld - 1)
                    {
                        NotesHeld[i] = NotesHeld[i + 1];
                    }
                    else
                    {
                        NotesHeld[i] = null;
                    }
                }
            }

            if(found)
            {
                totalNotesHeld--;
            }
        }
    }
}
