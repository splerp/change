using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace MonoJam
{
    public class GameController
    {
        public const int MAX_ENEMIES = 20;
        public const int COINS_PER_LAYER = 1000;
        public int totalEnemies;

        private MonoJam mj;
        public ShakeController mainShaker;
        public MainMenuController mainMenu;

        public Player player;
        public Enemy[] enemies;
        public List<Coin> coins;
        public List<EnemyCorpse> corpses;
        public List<Note> notes;

        public byte[] coinData;

        public static Random random;
        public bool ReadyToSpawnCoins { get; set; }
        public bool ReadyToSpawnEnemy { get; set; }
        public bool ReadyToSpawnNote { get; set; }

        Timer coinSpawner;
        Timer enemySpawner;
        Timer noteSpawner;

        public int placedCoins;
        public int currentCoins;
        public int coinsToSpawn;

        public bool IsPlaying { get; set; }

        public GameController(MonoJam mjIn)
        {
            mj = mjIn;
            mainShaker = new ShakeController();
            mainMenu = new MainMenuController(this);

            player = new Player(this);
            enemies = new Enemy[MAX_ENEMIES];

            coins = new List<Coin>();
            corpses = new List<EnemyCorpse>();
            notes = new List<Note>();
            random = new Random();

            coinSpawner = new Timer(1);
            coinSpawner.Elapsed += (a, b) => ReadyToSpawnCoins = true;

            enemySpawner = new Timer(1500);
            enemySpawner.Elapsed += (a, b) => ReadyToSpawnEnemy = true;

            noteSpawner = new Timer(2200);
            noteSpawner.Elapsed += (a, b) => ReadyToSpawnNote = true;
        }

        public void StartGame()
        {
            enemySpawner.Start();
            coinSpawner.Start();
            noteSpawner.Start();

            ResetCoinData();

            player.Reset();

            IsPlaying = true;
        }

        public void EndGame()
        {
            enemySpawner.Stop();
            coinSpawner.Stop();
            noteSpawner.Stop();

            currentCoins = 0;
            coinsToSpawn = 0;
            placedCoins = 0;
            
            DestroyAllEnemies();
            DestroyAllCoins();
            DestroyAllMoney();

            IsPlaying = false;
        }

        public void Exit()
        {
            mj.Exit();
        }

        public void Update()
        {
            // Always keep console window clear.
            Console.Clear();

            if(IsPlaying)
            {
                UpdatePlay();
            }
            else
            {
                mainMenu.Update();
            }
            
            #region Remove objects
            // Remove destroyed coins.
            for (int i = coins.Count - 1; i >= 0; i--)
            {
                for(int m = 0; m < coins[i].fallBy; m++)
                {
                    coins[i].MoveBy(new Vector2(0, 1));

                    if (coins[i].MoveAndCheckLand(coinData))
                    {
                        var pos = coins[i].CollisionRect.Location;
                        int arrayLoc = pos.Y * MonoJam.PLAYABLE_AREA_WIDTH + pos.X;

                        var coinArray = Enumerable.Repeat((byte)1, Coin.COIN_WIDTH).ToArray();

                        Buffer.BlockCopy(coinArray, 0, coinData, arrayLoc, coinArray.Length);

                        coins.RemoveAt(i);

                        placedCoins++;
                        break;
                    }
                }

                // Move coin buffers if required.
                if(placedCoins >= COINS_PER_LAYER)
                {
                    placedCoins = 0;
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
                    AddCoins(100);

                    var newCorpse = new EnemyCorpse(enemies[i]);
                    corpses.Add(newCorpse);

                    DestroyEnemy(enemies[i]);
                }
                else if (enemies[i].ReadyToRemove)
                {
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
                // TODO: Set on fire. No money earnt.
                if (notes[i].IsDead)
                {
                    notes.RemoveAt(i);
                }
                // Otherwise, if reached bottom, give money.
                else if (notes[i].ReadyToRemove)
                {
                    AddCoins(500);

                    notes.RemoveAt(i);
                }
            }

            // TODO: Explode the money
            #endregion

            mainShaker.Update();

            if(corpses.Any())
            {
                mainShaker.currentAmplitude = 1f;
            }

            Console.WriteLine($"COINS: {currentCoins}; to spawn: {coinsToSpawn} (on screen: {coins.Count})");
            Console.WriteLine("Corpses: " + corpses.Count);
        }

        public void UpdatePlay()
        {
            #region Create objects
            if (ReadyToSpawnCoins)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (coinsToSpawn > 0)
                    {
                        coinsToSpawn--;

                        // TODO: "Trend" coins into piles
                        var newCoin = new Coin();
                        newCoin.SetX(random.Next(0, MonoJam.PLAYABLE_AREA_WIDTH - Coin.COIN_WIDTH + 1));
                        coins.Add(newCoin);

                        ReadyToSpawnCoins = false;
                    }
                }
            }

            if (ReadyToSpawnEnemy)
            {
                SpawnEnemy();

                ReadyToSpawnEnemy = false;
            }

            if (ReadyToSpawnNote)
            {
                SpawnNote();

                ReadyToSpawnNote = false;
            }
            #endregion

            #region Update objects
            player.Update();

            // Update enemies.
            for (int i = 0; i < totalEnemies; i++)
            {
                enemies[i].Update();
            }

            for (int i = 0; i < notes.Count; i++)
            {
                notes[i].Update();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                EndGame();
            }

            foreach (var c in corpses)
            {
                c.Update();
            }
            #endregion
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

            mj.grc.ResetCoinBuffers();

            ResetCoinData();
        }

        public void DestroyAllMoney()
        {
            notes.Clear();
        }

        public void SpawnEnemy()
        {
            if (totalEnemies < MAX_ENEMIES)
            {
                var newEnemy = new Enemy();
                enemies[totalEnemies++] = newEnemy;
            }
            else
            {
                // Temporary for debugging
                throw new Exception("Created too many enemies");
            }
        }

        public void SpawnNote()
        {
            var newNote = new Note();
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
