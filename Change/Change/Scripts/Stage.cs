﻿using Splerp.Change.GameObjects;
using System;
using System.Collections.Generic;

namespace Splerp.Change
{
    // Defines a "level" in the game.
    public sealed class Stage
    {
        // The various level feature toggles.
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

        // Reset any values that may have changed. Important when
        // a stage is used again (e.g. on a second playthrough)
        public void Restart()
        {
            coinsCollected = 0;
            startTime = DateTime.Now;
        }

        public bool HasFlag(StageFlags flag)
        {
            return Flags.HasFlag(flag);
        }

        // Check whether this stage is over, and the next one should be loaded.
        public bool IsComplete()
        {
            bool complete = false;

            // If collected enough coins, stage is complete.
            if (HasFlag(StageFlags.CompleteOnCollectCoins))
            {
                if (coinsCollected >= RequiredCoins)
                {
                    complete = true;
                }
            }

            // If enough time has passed, stage is complete.
            if (HasFlag(StageFlags.CompleteOnTimePassed))
            {
                if (DateTime.Now - startTime > RequiredTimePassed)
                {
                    complete = true;
                }
            }

            return complete;
        }

        // For a given stage, find the succeeding stage.
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
            AvailableNotes = new Note.NoteType[] { Note.NoteType.Pink5, Note.NoteType.Blue10 },
            Flags = StageFlags.PaddlePlayerEnabled | StageFlags.NotesEnabled | StageFlags.CompleteOnCollectCoins,
            RequiredCoins = 2500,
            NoteSpawnTime = 3000,
            MaxNotesMissed = 5
        };

        public static Stage Tutorial2 = new Stage(true)
        {
            AvailableNotes = new Note.NoteType[] { Note.NoteType.Pink5, Note.NoteType.Blue10 },
            Flags = StageFlags.LaserPlayerEnabled | StageFlags.VacuumsEnabled | StageFlags.PigsEnabled | StageFlags.CompleteOnTimePassed,
            VacuumSpawnTime = 1500,
            RequiredTimePassed = TimeSpan.FromSeconds(45),
            MaxNotesMissed = 1
        };

        public static Stage Level1 = new Stage()
        {
            AvailableNotes = new Note.NoteType[] { Note.NoteType.Pink5, Note.NoteType.Blue10, Note.NoteType.Red20 },
            Flags = StageFlags.PaddlePlayerEnabled | StageFlags.NotesEnabled | StageFlags.PigsEnabled | StageFlags.LaserPlayerEnabled | StageFlags.VacuumsEnabled | StageFlags.CompleteOnTimePassed,
            RequiredTimePassed = TimeSpan.FromSeconds(40),
            NoteSpawnTime = 3000,
            VacuumSpawnTime = 5000,
            MaxNotesMissed = 20
        };

        public static Stage Level2 = new Stage()
        {
            AvailableNotes = new Note.NoteType[] { Note.NoteType.Pink5, Note.NoteType.Blue10, Note.NoteType.Red20, Note.NoteType.Yellow50, Note.NoteType.Green100 },
            Flags = StageFlags.PaddlePlayerEnabled | StageFlags.NotesEnabled | StageFlags.PigsEnabled | StageFlags.LaserPlayerEnabled | StageFlags.VacuumsEnabled | StageFlags.CompleteOnTimePassed,
            RequiredTimePassed = TimeSpan.FromSeconds(50),
            NoteSpawnTime = 2000,
            VacuumSpawnTime = 3000,
            MaxNotesMissed = 20
        };

        public static Stage Level3 = new Stage()
        {
            AvailableNotes = new Note.NoteType[] { Note.NoteType.Pink5, Note.NoteType.Blue10, Note.NoteType.Red20, Note.NoteType.Yellow50, Note.NoteType.Green100 },
            Flags = StageFlags.PaddlePlayerEnabled | StageFlags.NotesEnabled | StageFlags.PigsEnabled | StageFlags.LaserPlayerEnabled | StageFlags.VacuumsEnabled | StageFlags.CompleteOnTimePassed,
            RequiredTimePassed = TimeSpan.FromSeconds(60),
            NoteSpawnTime = 1000,
            VacuumSpawnTime = 2000,
            MaxNotesMissed = 20
        };

        public static Stage Level4 = new Stage()
        {
            AvailableNotes = new Note.NoteType[] { Note.NoteType.Pink5, Note.NoteType.Blue10, Note.NoteType.Red20, Note.NoteType.Yellow50, Note.NoteType.Green100 },
            Flags = StageFlags.PaddlePlayerEnabled | StageFlags.NotesEnabled | StageFlags.PigsEnabled | StageFlags.LaserPlayerEnabled | StageFlags.VacuumsEnabled | StageFlags.CompleteOnTimePassed,
            RequiredTimePassed = TimeSpan.FromSeconds(60),
            NoteSpawnTime = 900,
            VacuumSpawnTime = 1500,
            MaxNotesMissed = 10
        };

        public static Stage Level5 = new Stage()
        {
            AvailableNotes = new Note.NoteType[] { Note.NoteType.Pink5, Note.NoteType.Blue10, Note.NoteType.Red20, Note.NoteType.Yellow50, Note.NoteType.Green100 },
            Flags = StageFlags.PaddlePlayerEnabled | StageFlags.NotesEnabled | StageFlags.PigsEnabled | StageFlags.LaserPlayerEnabled | StageFlags.VacuumsEnabled | StageFlags.CompleteOnTimePassed,
            RequiredTimePassed = TimeSpan.FromSeconds(60),
            NoteSpawnTime = 600,
            VacuumSpawnTime = 500,
            MaxNotesMissed = 10
        };
        #endregion
    }
}
