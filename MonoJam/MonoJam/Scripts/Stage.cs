using MonoJam.GameObjects;
using System;
using System.Collections.Generic;

namespace MonoJam
{
    public class Stage
    {
        [Flags]
        public enum StageFlags
        {
            None = 0 << -1,
            PigsEnabled = 1 << 0,
            NotesEnabled = 1 << 1,
            VacuumsEnabled = 1 << 2,
            PaddlePlayerEnabled = 1 << 3,
            LaserPlayerEnabled = 1 << 4,
            CompleteOnCollectCoins = 1 << 5,
            CompleteOnTimePassed = 1 << 6,
        }
        
        public Note.NoteType[] AvailableNotes { get; set; }
        public int NoteSpawnTime { get; set; }
        public int VacuumSpawnTime { get; set; }
        public int RequiredCoins { get; set; }
        public int MaxNotesMissed { get; set; }
        public TimeSpan RequiredTimePassed { get; set; }
        public StageFlags Flags { get; set; }
        public Int64 coinsCollected;
        public DateTime startTime;

        private Stage(bool isTutorial = false)
        {
            AllStages.Add(this);

            if(!isTutorial)
            {
                NonTutorialStages.Add(this);
            }

            Restart();
        }

        public void Restart()
        {
            coinsCollected = 0;
            startTime = DateTime.Now;
        }

        public bool HasFlag(StageFlags flag)
        {
            return Flags.HasFlag(flag);
        }

        public bool IsComplete()
        {
            bool complete = false;

            if (HasFlag(StageFlags.CompleteOnCollectCoins))
            {
                if (coinsCollected >= RequiredCoins)
                {
                    complete = true;
                }
            }

            if (HasFlag(StageFlags.CompleteOnTimePassed))
            {
                if (DateTime.Now - startTime > RequiredTimePassed)
                {
                    complete = true;
                }
            }

            return complete;
        }

        public static Stage NextStage(Stage current)
        {
            // Don't check final stage (there will be no next stage for it).
            for(int i = 0; i < AllStages.Count - 1; i++)
            {
                if(current == AllStages[i])
                {
                    return AllStages[i + 1];
                }
            }

            return null;
        }

        #region All stages
        public static List<Stage> AllStages = new List<Stage>();
        public static List<Stage> NonTutorialStages = new List<Stage>();

        public static Stage Tutorial1 = new Stage(true)
        {
            AvailableNotes = new Note.NoteType[] { Note.NoteType.Pink5 },
            Flags = StageFlags.PaddlePlayerEnabled | StageFlags.NotesEnabled | StageFlags.CompleteOnCollectCoins,
            RequiredCoins = 1500,
            NoteSpawnTime = 3000,
            MaxNotesMissed = 5
        };

        public static Stage Tutorial2 = new Stage(true)
        {
            AvailableNotes = new Note.NoteType[] { Note.NoteType.Pink5 },
            Flags = StageFlags.LaserPlayerEnabled | StageFlags.VacuumsEnabled | StageFlags.PigsEnabled | StageFlags.CompleteOnTimePassed,
            VacuumSpawnTime = 1500,
            RequiredTimePassed = TimeSpan.FromSeconds(35)
        };

        public static Stage Level1 = new Stage()
        {
            AvailableNotes = new Note.NoteType[] { Note.NoteType.Pink5, Note.NoteType.Blue10, Note.NoteType.Red20, Note.NoteType.Yellow50 },
            Flags = StageFlags.PaddlePlayerEnabled | StageFlags.NotesEnabled | StageFlags.PigsEnabled | StageFlags.LaserPlayerEnabled | StageFlags.VacuumsEnabled | StageFlags.CompleteOnTimePassed,
            RequiredTimePassed = TimeSpan.FromSeconds(20),
            NoteSpawnTime = 3000,
            VacuumSpawnTime = 5000,
            MaxNotesMissed = 5
        };

        public static Stage Level2 = new Stage()
        {
            AvailableNotes = new Note.NoteType[] { Note.NoteType.Pink5, Note.NoteType.Blue10, Note.NoteType.Red20, Note.NoteType.Yellow50 },
            Flags = StageFlags.PaddlePlayerEnabled | StageFlags.NotesEnabled | StageFlags.PigsEnabled | StageFlags.LaserPlayerEnabled | StageFlags.VacuumsEnabled | StageFlags.CompleteOnTimePassed,
            RequiredTimePassed = TimeSpan.FromSeconds(20),
            NoteSpawnTime = 1000,
            VacuumSpawnTime = 3000,
            MaxNotesMissed = 5
        };
        #endregion
    }
}
