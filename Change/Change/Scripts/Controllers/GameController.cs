using Microsoft.Xna.Framework;
using Splerp.Change.GameObjects;
using Splerp.Change.Menus;
using Splerp.Change.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace Splerp.Change.Controllers
{
    public sealed class GameController : IDisposable
    {
        #region Delegates
        public delegate void StartEnter();
        public delegate void StageUpdate(GameTime gameTime);
        #endregion

        // Random object used everywhere.
        public static Random random = new Random();

        private ChangeGame cg;

        // Controllers.
        private GraphicsController grc;
        private InputController ic;
        private CoinBackgroundController coinBackgroundController;

        // Menu references.
        private MainMenu mainMenu;
        private GameOverMenu gameOverMenu;
        private StageCompleteMenu stageCompleteMenu;

        // The stage the player is currently on.
        public Stage CurrentStage { get; private set; }

        // The state the game is currently in.
        public GameState CurrentState { get; private set; }

        // Pass-through helpers, for classes that only need to access certain features.
        public List<Coin> FallingCoins => coinBackgroundController.coins;
        public Menu CurrentMenu
        {
            get
            {
                if (CurrentState == GameState.Title)
                {
                    return mainMenu;
                }
                else if (CurrentState == GameState.GameOver)
                {
                    return gameOverMenu;
                }
                else if (CurrentState == GameState.BetweenStages)
                {
                    return stageCompleteMenu;
                }
                else 
                {
                    return null;
                }
            }
        }

        // Unsorted
        public int notesMissed;
        public bool skipTutorial;

        #region Game Objects
        // Reference to laser controller.
        public LaserPlayer laserPlayer;

        // Reference to paddle controller
        public PaddlePlayer paddlePlayer;

        // All game object collections.
        public List<Enemy> enemies;
        public List<EnemyCorpse> corpses;
        public List<Note> notes;
        public List<NoteOnFire> notesOnFire;
        #endregion

        #region Timer variables
        private Timer piggyBankSpawner;
        private Timer vacuumSpawner;
        private Timer noteSpawner;
        private bool ReadyToSpawnPiggyBank { get; set; }
        private bool ReadyToSpawnVacuum { get; set; }
        private bool ReadyToSpawnNote { get; set; }
        #endregion

        #region Coin Counting
        public Int64 currentCoinScore;
        public Int64 bestCoinScore;
        #endregion

        public GameController(ChangeGame cgIn, GraphicsController grcIn, InputController icIn)
        {
            // Set references.
            cg = cgIn;
            grc = grcIn;
            ic = icIn;

            // Add reference to graphics controller.
            grc.SetGameControllerReference(this);

            // Define state definitions (with references to this GameController instance's content).
            #region State definitions
            GameState.Title = new GameState("title")
            {
                OnEnterState = ResetContent,
                OnStateUpdate = UpdateMainMenu
            };
            GameState.MapControls = new GameState("mapcontrols")
            {
                OnEnterState = StartMappingControls,
                OnStateUpdate = UpdateMapControls
            };
            GameState.Playing = new GameState("playing")
            {
                OnEnterState = OnStartPlaying,
                OnStateUpdate = UpdatePlay
            };
            GameState.BetweenStages = new GameState("betweenstages")
            {
                OnEnterState = () => { },
                OnStateUpdate = (GameTime gameTime) =>
                {
                    UpdatePlay(gameTime);
                    UpdateBetweenStages(gameTime);
                }
            };
            GameState.GameOver = new GameState("gameover")
            {
                OnEnterState = () =>
                {
                    gameOverMenu.Drop();

                    bestCoinScore = Math.Max(bestCoinScore, currentCoinScore);

                    laserPlayer.FiringLaser = false;
                    SoundController.StopAllLoops();
                },
                OnStateUpdate = UpdateGameOverMenu
            };
            #endregion

            // Set up controllers.
            coinBackgroundController = new CoinBackgroundController();

            // Set up menus.
            mainMenu = new MainMenu(cg, this);
            gameOverMenu = new GameOverMenu(this);
            stageCompleteMenu = new StageCompleteMenu();

            // Set up players.
            laserPlayer = new LaserPlayer(this);
            paddlePlayer = new PaddlePlayer();

            // Define collections.
            enemies = new List<Enemy>();
            corpses = new List<EnemyCorpse>();
            notes = new List<Note>();
            notesOnFire = new List<NoteOnFire>();

            // Define timers.
            piggyBankSpawner = new Timer(3000);
            piggyBankSpawner.Elapsed += (a, b) => ReadyToSpawnPiggyBank = true;

            vacuumSpawner = new Timer();
            vacuumSpawner.Elapsed += (a, b) => ReadyToSpawnVacuum = true;

            noteSpawner = new Timer();
            noteSpawner.Elapsed += (a, b) => ReadyToSpawnNote = true;
        }
        
        public void Update(GameTime gameTime)
        {
            // Do state-specific update logic.
            CurrentState.OnStateUpdate(gameTime);

            // Update current menu.
            CurrentMenu?.Update(gameTime);

            #region Global input handling
            if (Control.MuteSound.IsJustPressed)
            {
                SoundController.ToggleMute();
                SoundController.Play(Sound.Bip2);
            }

            if (Control.MuteMusic.IsJustPressed)
            {
                SoundController.ToggleMuteMusic();
                SoundController.Play(Sound.Bip2);
            }

            if (Control.SkipTutorial.IsJustPressed)
            {
                skipTutorial = !skipTutorial;
                SoundController.Play(Sound.Bip2);
            }
            #endregion
        }

        #region State methods
        public void SetState(GameState newState)
        {
            // HACK: special case for between stages -> playing, don't reset content.
            if (!(newState == GameState.Playing && CurrentState == GameState.BetweenStages))
            {
                newState.OnEnterState();
            }

            CurrentState = newState;
        }

        #region State "On Start" methods
        public void OnStartPlaying()
        {
            ResetContent();

            piggyBankSpawner.Start();
            vacuumSpawner.Start();
            noteSpawner.Start();

            laserPlayer.Reset();
            paddlePlayer.Reset();

            if (skipTutorial)
            {
                CurrentStage = Stage.NonTutorialStages.First();
            }
            else
            {
                CurrentStage = Stage.AllStages.First();
            }

            SetStageVariables();
        }

        public void StartMappingControls()
        {
            ic.StartRemapping();
        }
        #endregion

        #region State "Update" methods
        public void UpdatePlay(GameTime gameTime)
        {
            #region Create objects
            if (ReadyToSpawnPiggyBank)
            {
                SpawnPiggyBank();

                ReadyToSpawnPiggyBank = false;
            }

            if (ReadyToSpawnVacuum)
            {
                SpawnVacuum();

                ReadyToSpawnVacuum = false;
            }

            if (ReadyToSpawnNote)
            {
                SpawnNote();

                ReadyToSpawnNote = false;
            }
            #endregion

            #region Update objects
            if (CurrentStage.HasFlag(Stage.StageFlags.LaserPlayerEnabled))
            {
                laserPlayer.Update(gameTime);
                laserPlayer.FixedUpdate();
            }
            if (CurrentStage.HasFlag(Stage.StageFlags.PaddlePlayerEnabled))
            {
                paddlePlayer.Update(gameTime);
            }

            coinBackgroundController.Update(gameTime);

            // Update enemies.
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].Update(gameTime);

                if (enemies[i] is VacuumEnemy)
                {
                    var vEnemy = enemies[i] as VacuumEnemy;

                    for (int j = notes.Count - 1; j >= 0; j--)
                    {
                        var note = notes[j];

                        vEnemy.TryCollect(note);
                    }
                }
            }

            for (int i = 0; i < notes.Count; i++)
            {
                notes[i].Update(gameTime);
            }

            foreach (var c in corpses)
            {
                c.Update(gameTime);
            }

            foreach (var c in notesOnFire)
            {
                c.Update(gameTime);
            }

            CurrentStage.Update(gameTime);
            #endregion

            #region Remove objects
            // Remove destroyed enemies.
            for (int i = 0; i < enemies.Count; i++)
            {
                // A "killed" enemy. Show the death sequence.
                if (enemies[i].IsDead)
                {
                    AddCoins(enemies[i].CoinsOnDeath);

                    var newCorpse = new EnemyCorpse(enemies[i]);
                    corpses.Add(newCorpse);

                    enemies[i].OnDeath();

                    DestroyEnemy(enemies[i]);
                }
                else if (enemies[i].ReadyToRemove)
                {
                    if (enemies[i] is VacuumEnemy)
                    {
                        // Only play sound once.
                        if (((VacuumEnemy)enemies[i]).totalNotesHeld > 0)
                        {
                            SoundController.Play(Sound.FireFlare);
                        }

                        for (int j = ((VacuumEnemy)enemies[i]).totalNotesHeld - 1; j >= 0; j--)
                        {
                            var note = ((VacuumEnemy)enemies[i]).NotesHeld[j];

                            notes.Remove(note);
                            notesMissed++;
                        }
                    }

                    DestroyEnemy(enemies[i]);
                }
            }

            for (int i = corpses.Count - 1; i >= 0; i--)
            {
                if (corpses[i].ReadyToRemove)
                {
                    corpses.RemoveAt(i);
                }
            }

            for (int i = notes.Count - 1; i >= 0; i--)
            {
                // Set on fire. No money earnt.
                if (notes[i].IsDead)
                {
                    var newOnFire = new NoteOnFire(notes[i]);
                    notesOnFire.Add(newOnFire);

                    if (notes[i].CaughtByVacuum != null)
                    {
                        notes[i].CaughtByVacuum.RemoveNoteFromVacuum(notes[i]);
                    }

                    SoundController.Play(Sound.FireFlare);

                    notes.RemoveAt(i);

                    notesMissed++;
                }
                // Otherwise, if caught, give money.
                else if (CurrentStage.HasFlag(Stage.StageFlags.PaddlePlayerEnabled) && notes[i].InRangeForCatching && !notes[i].CaughtByPlayer && BoxCollisionTest.IntersectAABB(paddlePlayer.CollisionRect, notes[i].CollisionRect))
                {
                    notes[i].CaughtByPlayer = true;
                }
                // Otherwise, check if ready to remove
                else if (notes[i].ReadyToRemove)
                {
                    if (notes[i].CaughtByPlayer)
                    {
                        AddCoins(Note.noteWorths[notes[i].Type]);
                        SoundController.Play(Sound.Ding);
                    }
                    else
                    {
                        notesMissed++;

                        SoundController.Play(Sound.FireFlare);
                    }

                    notes.RemoveAt(i);
                }
            }

            for (int i = notesOnFire.Count - 1; i >= 0; i--)
            {
                if (notesOnFire[i].ReadyToRemove)
                {
                    notesOnFire.RemoveAt(i);
                }
            }
            #endregion

            // If run out of health, game over.
            if (CurrentStage.HasFlag(Stage.StageFlags.NotesEnabled) && notesMissed >= CurrentStage.MaxNotesMissed)
            {
                SetState(GameState.GameOver);
            }

            // Move on to the next stage.
            if (CurrentState == GameState.Playing && CurrentStage.IsComplete())
            {
                ToNextStage();
            }

            if (Control.Return.IsJustPressed)
            {
                SetState(GameState.GameOver);
            }
        }

        public void UpdateMainMenu(GameTime gameTime)
        {
            if (Control.Return.IsJustPressed)
            {
                cg.Exit();
            }
        }

        public void UpdateGameOverMenu(GameTime gameTime)
        {
            if (Control.Return.IsJustPressed)
            {
                SetState(GameState.Title);
            }
        }

        public void UpdateBetweenStages(GameTime gameTime)
        {
            if (stageCompleteMenu.AnimationComplete)
            {
                ToNextStageFinish();
            }
        }

        public void UpdateMapControls(GameTime gameTime)
        {
            ic.UpdateMapControls(this);
        }
        #endregion
        #endregion

        #region Stage methods
        public void ToNextStage()
        {
            SetState(GameState.BetweenStages);
            stageCompleteMenu.Drop();

            RemoveStageContent();
        }

        public void ToNextStageFinish()
        {
            var newStage = Stage.NextStage(CurrentStage);

            if (newStage != null)
            {
                CurrentStage = newStage;
                SetStageVariables();
                SetState(GameState.Playing);
            }
            else
            {
                SetState(GameState.GameOver);
            }
        }

        public void SetStageVariables()
        {
            CurrentStage.Restart();

            notesMissed = 0;

            piggyBankSpawner.Stop();
            if (CurrentStage.HasFlag(Stage.StageFlags.PigsEnabled))
            {
                piggyBankSpawner.Start();
            }

            vacuumSpawner.Stop();
            if (CurrentStage.HasFlag(Stage.StageFlags.VacuumsEnabled))
            {
                vacuumSpawner.Interval = CurrentStage.VacuumSpawnTime;
                vacuumSpawner.Start();
            }

            noteSpawner.Stop();
            if (CurrentStage.HasFlag(Stage.StageFlags.NotesEnabled))
            {
                noteSpawner.Interval = CurrentStage.NoteSpawnTime;
                noteSpawner.Start();
            }

            if (!CurrentStage.HasFlag(Stage.StageFlags.LaserPlayerEnabled))
            {
                laserPlayer.FiringLaser = false;
                SoundController.StopAllLoops();
            }
        }
        #endregion

        public void AddCoins(int coins)
        {
            currentCoinScore += coins;
            coinBackgroundController.coinsToSpawn += coins;
            CurrentStage.coinsCollected += coins;
        }

        #region Spawner methods
        public void SpawnPiggyBank()
        {
            var newPig = new PiggyBank();
            newPig.Init();
            enemies.Add(newPig);
        }

        public void SpawnVacuum()
        {
            var newVacuum = new VacuumEnemy();
            newVacuum.Init();
            enemies.Add(newVacuum);
        }

        public void SpawnNote()
        {
            var totalWeights = Note.noteSpawnWeights
                .Where(a => CurrentStage.AvailableNotes.Contains(a.Key))
                .Sum(a => a.Value);
            var idx = random.Next(0, totalWeights);

            int i = 0;
            Note.NoteType selectedType = Note.NoteType.None;
            while (idx > 0)
            {
                idx -= Note.noteSpawnWeights[CurrentStage.AvailableNotes[i]];

                selectedType = CurrentStage.AvailableNotes[i];
                i++;
            }

            notes.Add(new Note(this, selectedType));
        }
        #endregion

        #region Cleanup methods
        // Completely clears the level (ready to play from scratch).
        public void ResetContent()
        {
            RemoveStageContent();

            currentCoinScore = 0;

            ReadyToSpawnPiggyBank = false;
            ReadyToSpawnVacuum = false;
            ReadyToSpawnNote = false;

            coinBackgroundController.Reset();
            grc.ResetCoinBuffers();

            laserPlayer.FiringLaser = false;
            SoundController.StopAllLoops();
        }

        // Clears all content between stages.
        public void RemoveStageContent()
        {
            piggyBankSpawner.Stop();
            vacuumSpawner.Stop();
            noteSpawner.Stop();

            notesMissed = 0;

            DestroyAllEnemies();
            DestroyAllMoney();
        }

        public void DestroyAllMoney()
        {
            notes.Clear();
            notesOnFire.Clear();
        }

        public void DestroyAllEnemies()
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                DestroyEnemy(enemies[i]);
            }

            // And remove all corpses.
            corpses.Clear();
        }

        public void DestroyEnemy(Enemy e)
        {
            enemies.Remove(e);
        }
        #endregion

        #region Disposal handling
        ~GameController()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if(disposing)
            {
                piggyBankSpawner.Dispose();
                vacuumSpawner.Dispose();
                noteSpawner.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
