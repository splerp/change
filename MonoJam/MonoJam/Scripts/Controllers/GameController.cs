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
        public enum GameState { Title, Playing, GameOver }

        public const int MAX_ENEMIES = 20;
        public const int COINS_PER_LAYER = 5000;
        public const int MAX_NOTES_MISSED = 20;
        
        public const int COINS_SPAWNED_PER_FRAME = 8;

        public const float SMALL_SHAKE_AMOUNT = 1f;

        public int totalEnemies;

        private MonoJam mj;
        public ShakeController mainShaker;
        public MainMenuController mainMenu;
        public GameOverMenuController gameOverMenu;

        public LaserPlayer laserPlayer;
        public PaddlePlayer paddlePlayer;
        public Enemy[] enemies;
        public List<Coin> coins;
        public List<EnemyCorpse> corpses;
        public List<Note> notes;
        public List<NoteOnFire> notesOnFire;

        public byte[] coinData;

        public static Random random;
        public bool ReadyToSpawnCoins { get; set; }
        public bool ReadyToSpawnPiggyBank { get; set; }
        public bool ReadyToSpawnVacuum { get; set; }
        public bool ReadyToSpawnNote { get; set; }

        public int[] currentLayerTrend;
        public int totalTrendCount;

        Timer coinSpawner;
        Timer piggyBankSpawner;
        Timer vacuumSpawner;
        Timer noteSpawner;

        public Int64 placedCoins;
        public Int64 currentCoins;
        public Int64 coinsToSpawn;
        public int notesMissed;

        public bool coinsStartFalling;
        public bool coinsStopFalling;

        public GameState currentState;

        public GameController(MonoJam mjIn)
        {
            mj = mjIn;
            mainShaker = new ShakeController();
        }

        public void Init()
        {
            mainMenu = new MainMenuController(this);
            gameOverMenu = new GameOverMenuController(this);

            laserPlayer = new LaserPlayer(this);
            paddlePlayer = new PaddlePlayer(this);

            enemies = new Enemy[MAX_ENEMIES];

            coins = new List<Coin>();
            corpses = new List<EnemyCorpse>();
            notes = new List<Note>();
            notesOnFire = new List<NoteOnFire>();
            random = new Random();

            coinSpawner = new Timer(1);
            coinSpawner.Elapsed += (a, b) => ReadyToSpawnCoins = true;

            piggyBankSpawner = new Timer(10000);
            piggyBankSpawner.Elapsed += (a, b) => ReadyToSpawnPiggyBank = true;

            vacuumSpawner = new Timer(5000);
            vacuumSpawner.Elapsed += (a, b) => ReadyToSpawnVacuum = true;
            
            noteSpawner = new Timer(1000);
            noteSpawner.Elapsed += (a, b) => ReadyToSpawnNote = true;

            currentLayerTrend = new int[MonoJam.PLAYABLE_AREA_WIDTH];
            
            ToMainMenu();
        }

        public void StartGame()
        {
            piggyBankSpawner.Start();
            coinSpawner.Start();
            vacuumSpawner.Start();
            noteSpawner.Start();

            ResetCoinData();

            laserPlayer.Reset();
            paddlePlayer.Reset();
            CalculateCoinTrend();

            currentState = GameState.Playing;
        }

        public void ResetContent()
        {
            piggyBankSpawner.Stop();
            coinSpawner.Stop();
            vacuumSpawner.Stop();
            noteSpawner.Stop();

            currentCoins = 0;
            coinsToSpawn = 0;
            placedCoins = 0;
            notesMissed = 0;

            ReadyToSpawnCoins = false;
            ReadyToSpawnPiggyBank = false;
            ReadyToSpawnVacuum = false;
            ReadyToSpawnNote = false;
       
            DestroyAllEnemies();
            DestroyAllCoins();
            DestroyAllMoney();
            mj.grc.ResetCoinBuffers();

            laserPlayer.FiringLaser = false;
            SoundController.StopAllLoops();
        }

        public void RestartGame()
        {
            ResetContent();
            StartGame();
        }

        public void ToGameOver()
        {
            gameOverMenu.Drop();
            currentState = GameState.GameOver;

            laserPlayer.FiringLaser = false;
            SoundController.StopAllLoops();
        }

        public void ToMainMenu()
        {
            ResetContent();
            currentState = GameState.Title;
        }

        public void Exit()
        {
            mj.Exit();
        }

        public void Update()
        {
            // Always keep console window clear.
            Console.Clear();

            switch(currentState)
            {
                case GameState.Title:
                    mainMenu.Update();
                    break;
                case GameState.Playing:
                    UpdatePlay();
                    break;
                case GameState.GameOver:
                    gameOverMenu.Update();
                    break;
            }

            mainShaker.Update();
            
            if(currentState != GameState.GameOver && notesMissed >= MAX_NOTES_MISSED)
            {
                ToGameOver();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                ToMainMenu();
            }

            Console.WriteLine($"COINS: {currentCoins}; to spawn: {coinsToSpawn} (on screen: {coins.Count})");
            Console.WriteLine("Corpses: " + corpses.Count);
            Console.WriteLine("Current state: " + currentState);
        }

        public void UpdatePlay()
        {
            #region Create objects
            if (ReadyToSpawnCoins)
            {
                if(coinsToSpawn > 0)
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
                        while(randomNum > 0)
                        {
                            randomNum -= currentLayerTrend[j];
                            j++;
                        }
                        
                        var newCoin = new Coin();
                        newCoin.SetX(j);
                        coins.Add(newCoin);

                        ReadyToSpawnCoins = false;
                    }
                }
            }

            if (ReadyToSpawnPiggyBank)
            {
                SpawnPiggyBank();

                ReadyToSpawnPiggyBank = false;
            }

            if(ReadyToSpawnVacuum)
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
            laserPlayer.Update();
            paddlePlayer.Update();

            // Update enemies.
            for (int i = 0; i < totalEnemies; i++)
            {
                enemies[i].Update();

                if(enemies[i] is VacuumEnemy)
                {
                    var vEnemy = enemies[i] as VacuumEnemy;

                    for(int j = notes.Count - 1; j >= 0; j--)
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

                    if(notes[i].CaughtByVacuum != null)
                    {
                        notes[i].CaughtByVacuum.RemoveNoteFromVacuum(notes[i]);
                    }
                    
                    SoundController.Play(Sound.FireFlare);

                    notes.RemoveAt(i);

                    notesMissed++;
                }
                // Otherwise, if caught, give money.
                else if (notes[i].InRangeForCatching && !notes[i].CaughtByPlayer && BoxCollisionTest.IntersectAABB(paddlePlayer.CollisionRect, notes[i].CollisionRect))
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

            if (corpses.Any())
            {
                //mainShaker.currentAmplitude = SMALL_SHAKE_AMOUNT;
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
            

            var totalWeights = Note.noteSpawnWeights.Sum(a => a.Value);
            var idx = random.Next(0, totalWeights);

            int i = 0;
            Note.NoteType selectedType = Note.NoteType.None;
            Note.NoteType[] allEnumValues = (Note.NoteType[])Enum.GetValues(typeof(Note.NoteType));
            while(idx > 0)
            {
                idx -= Note.noteSpawnWeights[allEnumValues[i]];

                selectedType = allEnumValues[i];
                i++;
            }

            var newNote = new Note(this, selectedType);

            notes.Add(newNote);
        }

        public void DestroyAllEnemies()
        {
            for(int i = totalEnemies - 1; i >= 0; i--)
            {
                DestroyEnemy(enemies[i]);
            }

            // And remove all corpses.
            corpses.Clear();
        }

        public void DestroyEnemy(Enemy e)
        {
            bool foundEnemy = false;

            for(int i = 0; i < totalEnemies; i++)
            {
                if(enemies[i] == e)
                {
                    foundEnemy = true;
                }

                // Shift remaining enemies down the array.
                if(foundEnemy)
                {
                    if(i + 1 < totalEnemies)
                    {
                        enemies[i] = enemies[i + 1];
                    }
                    else
                    {
                        enemies[i] = null;
                    }
                }
            }

            if(foundEnemy)
            {
                totalEnemies--;
            }
        }
    }
}
