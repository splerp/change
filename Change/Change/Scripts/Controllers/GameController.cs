using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoJam.GameObjects;
using MonoJam.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace MonoJam.Controllers
{
    public class GameController
    {
        public const int MAX_ENEMIES = 20;
        public const int COINS_PER_LAYER = 5000;
        public const int COINS_SPAWNED_PER_FRAME = 8;
        public const float SMALL_SHAKE_AMOUNT = 1f;

        public delegate void StartEnter();
        public delegate void StageUpdate();

        public int totalEnemies;

        public Stage currentStage;

        private MonoJam mj;
        public ShakeController mainShaker;
        public MainMenuController mainMenu;
        public GameOverMenuController gameOverMenu;
        public StageCompleteMenuController stageCompleteMenu;

        public LaserPlayer laserPlayer;
        public PaddlePlayer paddlePlayer;
        public Enemy[] enemies;
        public List<Coin> coins;
        public List<EnemyCorpse> corpses;
        public List<Note> notes;
        public List<NoteOnFire> notesOnFire;

        public byte[] coinData;

        public static Random random;
        public bool ReadyToSpawnPiggyBank { get; set; }
        public bool ReadyToSpawnVacuum { get; set; }
        public bool ReadyToSpawnNote { get; set; }

        public int[] currentLayerTrend;
        public int totalTrendCount;

        Timer piggyBankSpawner;
        Timer vacuumSpawner;
        Timer noteSpawner;

        public Int64 placedCoins;
        public Int64 currentCoins;
        public Int64 coinsToSpawn;
        public Int64 bestCoinScore;
        public int notesMissed;

        public bool coinsStartFalling;
        public bool coinsStopFalling;

        public bool skipTutorial;

        public GameState currentState;
        public bool pressingKeysLastFrame;

        public GameController(MonoJam mjIn)
        {
            mj = mjIn;
            mainShaker = new ShakeController();

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
                OnStateUpdate = () =>
                {
                    UpdatePlay();
                    UpdateBetweenStages();
                }
            };
            GameState.GameOver = new GameState("gameover")
            {
                OnEnterState = () =>
                {
                    gameOverMenu.Drop();

                    bestCoinScore = Math.Max(bestCoinScore, currentCoins);

                    laserPlayer.FiringLaser = false;
                    SoundController.StopAllLoops();
                },
                OnStateUpdate = UpdateGameOverMenu
            };
            #endregion
        }

        public void Init()
        {
            mainMenu = new MainMenuController(this);
            gameOverMenu = new GameOverMenuController(this);
            stageCompleteMenu = new StageCompleteMenuController(this);

            laserPlayer = new LaserPlayer(this);
            paddlePlayer = new PaddlePlayer(this);

            enemies = new Enemy[MAX_ENEMIES];

            coins = new List<Coin>();
            corpses = new List<EnemyCorpse>();
            notes = new List<Note>();
            notesOnFire = new List<NoteOnFire>();
            random = new Random();

            piggyBankSpawner = new Timer(30000);
            piggyBankSpawner.Elapsed += (a, b) => ReadyToSpawnPiggyBank = true;

            vacuumSpawner = new Timer(5000);
            vacuumSpawner.Elapsed += (a, b) => ReadyToSpawnVacuum = true;

            noteSpawner = new Timer(500);
            noteSpawner.Elapsed += (a, b) => ReadyToSpawnNote = true;

            currentLayerTrend = new int[MonoJam.PLAYABLE_AREA_WIDTH];

            SetState(GameState.Title);
        }

        public void SetState(GameState newState)
        {
            // HACK: sepcial case for between stages -> playing, don't reset content.
            if (!(newState == GameState.Playing && currentState == GameState.BetweenStages))
            {
                newState.OnEnterState();
            }

            currentState = newState;
        }

        public void OnStartPlaying()
        {
            ResetContent();

            piggyBankSpawner.Start();
            vacuumSpawner.Start();
            noteSpawner.Start();

            ResetCoinData();

            laserPlayer.Reset();
            paddlePlayer.Reset();
            CalculateCoinTrend();

            if (skipTutorial)
            {
                currentStage = Stage.NonTutorialStages.First();
            }
            else
            {
                currentStage = Stage.AllStages.First();
            }

            SetStageVariables();
        }

        public void ToNextStage()
        {
            SetState(GameState.BetweenStages);
            stageCompleteMenu.Drop();

            RemoveStageContent();
        }

        public void ToNextStageFinish()
        {
            var newStage = Stage.NextStage(currentStage);

            if (newStage != null)
            {
                currentStage = newStage;
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
            currentStage.Restart();

            notesMissed = 0;

            piggyBankSpawner.Stop();
            if (currentStage.HasFlag(Stage.StageFlags.PigsEnabled))
            {
                piggyBankSpawner.Start();
            }

            vacuumSpawner.Stop();
            if (currentStage.HasFlag(Stage.StageFlags.VacuumsEnabled))
            {
                vacuumSpawner.Interval = currentStage.VacuumSpawnTime;
                vacuumSpawner.Start();
            }

            noteSpawner.Stop();
            if (currentStage.HasFlag(Stage.StageFlags.NotesEnabled))
            {
                noteSpawner.Interval = currentStage.NoteSpawnTime;
                noteSpawner.Start();
            }

            if (!currentStage.HasFlag(Stage.StageFlags.LaserPlayerEnabled))
            {
                laserPlayer.FiringLaser = false;
                SoundController.StopAllLoops();
            }
        }

        public void RemoveStageContent()
        {
            piggyBankSpawner.Stop();
            vacuumSpawner.Stop();
            noteSpawner.Stop();

            notesMissed = 0;

            DestroyAllEnemies();
            DestroyAllMoney();
        }

        public void ResetContent()
        {
            RemoveStageContent();
            DestroyAllCoins();

            currentCoins = 0;
            coinsToSpawn = 0;
            placedCoins = 0;

            ReadyToSpawnPiggyBank = false;
            ReadyToSpawnVacuum = false;
            ReadyToSpawnNote = false;

            mj.grc.ResetCoinBuffers();

            laserPlayer.FiringLaser = false;
            SoundController.StopAllLoops();
        }

        public void StartMappingControls()
        {
            mj.ic.StartRemapping();
        }

        public void UpdateMapControls()
        {
            if(mj.ic.FinishedRemapping)
            {
                SetState(GameState.Title);
            }
            else
            {
                // Should properly support all supported modes (mouse, gamepad), not just keyboard.
                // Keep track of current control to override and listen for input.
                var pressedKeys = Keyboard.GetState().GetPressedKeys();

                var pressingKeyboard = pressedKeys.Length > 0;

                if (pressingKeyboard && !pressingKeysLastFrame)
                {
                    mj.ic.RemapControl(pressedKeys);
                }
            }
        }

        public void Exit()
        {
            mj.Exit();
        }

        public void Update()
        {
            currentState.OnStateUpdate();

            mainShaker.Update();

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

            //Console.WriteLine($"COINS: {currentCoins}; to spawn: {coinsToSpawn} (on screen: {coins.Count})");
            //Console.WriteLine($"COINS Best: {bestCoinScore}"); 
            //Console.WriteLine("Corpses: " + corpses.Count);
            //Console.WriteLine("Current state: " + currentState);

            var pressedKeys = Keyboard.GetState().GetPressedKeys();
            pressingKeysLastFrame = pressedKeys.Length > 0;
        }

        public void UpdateMainMenu()
        {
            mainMenu.Update();

            if (Control.Return.IsJustPressed)
            {
                Exit();
            }
        }

        public void UpdateGameOverMenu()
        {
            gameOverMenu.Update();

            if (Control.Return.IsJustPressed)
            {
                SetState(GameState.Title);
            }
        }

        public void UpdateBetweenStages()
        {
            stageCompleteMenu.Update();

            if (stageCompleteMenu.AnimationComplete)
            {
                ToNextStageFinish();
            }
        }

        public void UpdatePlay()
        {
            #region Create objects
            if (coinsToSpawn > 0)
            {
                SoundController.Play(Sound.CoinsDrop, true);

                coinsStartFalling = false;
                coinsStopFalling = true;
            }
            else
            {
                SoundController.Stop(Sound.CoinsDrop);

                coinsStartFalling = true;
                coinsStopFalling = false;
            }

            for (int i = 0; i < COINS_SPAWNED_PER_FRAME; i++)
            {
                if (coinsToSpawn > 0)
                {
                    coinsToSpawn--;

                    var randomNum = random.Next(0, totalTrendCount);
                    int j = 0;
                    while (randomNum > 0)
                    {
                        randomNum -= currentLayerTrend[j];
                        j++;
                    }

                    var newCoin = new Coin();
                    newCoin.SetX(j);
                    coins.Add(newCoin);
                }
            }

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

            if (currentStage.HasFlag(Stage.StageFlags.LaserPlayerEnabled))
            {
                laserPlayer.Update();
            }
            if (currentStage.HasFlag(Stage.StageFlags.PaddlePlayerEnabled))
            {
                paddlePlayer.Update();
            }

            // Update enemies.
            for (int i = 0; i < totalEnemies; i++)
            {
                enemies[i].Update();

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
                notes[i].Update();
            }

            foreach (var c in corpses)
            {
                c.Update();
            }

            foreach (var c in notesOnFire)
            {
                c.Update();
            }
            #endregion

            #region Remove objects
            // Remove destroyed coins.
            for (int i = coins.Count - 1; i >= 0; i--)
            {
                for (int m = 0; m < coins[i].fallBy; m++)
                {
                    coins[i].MoveBy(new Vector2(0, 1));

                    if (coins[i].MoveAndCheckLand(coinData))
                    {
                        var pos = coins[i].CollisionRect.Location;
                        int arrayLoc = pos.Y * MonoJam.PLAYABLE_AREA_WIDTH + pos.X;

                        coinData[arrayLoc] = 1;

                        coins.RemoveAt(i);

                        placedCoins++;
                        break;
                    }
                }

                // Move coin buffers if required.
                if (placedCoins >= COINS_PER_LAYER)
                {
                    placedCoins = 0;
                    CalculateCoinTrend();
                    mj.grc.CreateNewCoinBuffer();
                    ResetCoinData();
                }
            }

            // Remove destroyed enemies.
            for (int i = 0; i < totalEnemies; i++)
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
                else if (currentStage.HasFlag(Stage.StageFlags.PaddlePlayerEnabled) && notes[i].InRangeForCatching && !notes[i].CaughtByPlayer && BoxCollisionTest.IntersectAABB(paddlePlayer.CollisionRect, notes[i].CollisionRect))
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

            // TODO: Explode the money
            #endregion

            if (currentStage.HasFlag(Stage.StageFlags.NotesEnabled) && notesMissed >= currentStage.MaxNotesMissed)
            {
                SetState(GameState.GameOver);
            }

            if (currentState == GameState.Playing && currentStage.IsComplete())
            {
                ToNextStage();
            }

            if (Control.Return.IsJustPressed)
            {
                SetState(GameState.GameOver);
            }
        }

        public void CalculateCoinTrend()
        {
            float randomStartPos = random.Next(0, 100) / 100f;
            totalTrendCount = 0;
            for (int i = 0; i < currentLayerTrend.Length; i++)
            {
                float newVal = 5 + (Perlin.Noise(0, i * 0.05f + randomStartPos) * 5) + 1;
                currentLayerTrend[i] = (int)newVal;
                totalTrendCount += (int)newVal;
            }
        }

        public void AddCoins(int coins)
        {
            currentCoins += coins;
            coinsToSpawn += coins;
            currentStage.coinsCollected += coins;
        }

        public void ResetCoinData()
        {
            coinData = new byte[MonoJam.PLAYABLE_AREA_WIDTH * MonoJam.PLAYABLE_AREA_HEIGHT];
        }

        public void DestroyAllCoins()
        {
            coins.Clear();

            ResetCoinData();
        }

        public void DestroyAllMoney()
        {
            notes.Clear();
            notesOnFire.Clear();
        }

        public void SpawnPiggyBank()
        {
            if (totalEnemies < MAX_ENEMIES)
            {
                var newPig = new PiggyBank();
                enemies[totalEnemies++] = newPig;
            }
            else
            {
                // Temporary for debugging
                throw new Exception("Created too many enemies");
            }
        }

        public void SpawnVacuum()
        {
            if (totalEnemies < MAX_ENEMIES)
            {
                var newVacuum = new VacuumEnemy();
                enemies[totalEnemies++] = newVacuum;
            }
            else
            {
                // Temporary for debugging
                throw new Exception("Created too many enemies");
            }
        }

        public void SpawnNote()
        {
            var totalWeights = Note.noteSpawnWeights
                .Where(a => currentStage.AvailableNotes.Contains(a.Key))
                .Sum(a => a.Value);
            var idx = random.Next(0, totalWeights);

            int i = 0;
            Note.NoteType selectedType = Note.NoteType.None;
            while (idx > 0)
            {
                idx -= Note.noteSpawnWeights[currentStage.AvailableNotes[i]];

                selectedType = currentStage.AvailableNotes[i];
                i++;
            }

            notes.Add(new Note(this, selectedType));
        }

        public void DestroyAllEnemies()
        {
            for (int i = totalEnemies - 1; i >= 0; i--)
            {
                DestroyEnemy(enemies[i]);
            }

            // And remove all corpses.
            corpses.Clear();
        }

        public void DestroyEnemy(Enemy e)
        {
            bool foundEnemy = false;

            for (int i = 0; i < totalEnemies; i++)
            {
                if (enemies[i] == e)
                {
                    foundEnemy = true;
                }

                // Shift remaining enemies down the array.
                if (foundEnemy)
                {
                    if (i + 1 < totalEnemies)
                    {
                        enemies[i] = enemies[i + 1];
                    }
                    else
                    {
                        enemies[i] = null;
                    }
                }
            }

            if (foundEnemy)
            {
                totalEnemies--;
            }
        }
    }
}
