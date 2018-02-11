﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace MonoJam
{
    public class GameController
    {
        public const int MAX_ENEMIES = 500;
        public int totalEnemies;

        private MonoJam mj;

        public Player player;
        public Enemy[] enemies;
        public List<Coin> coins;

        public byte[] coinData;

        public static Random random;
        public bool ReadyToSpawnCoins { get; set; }
        public bool ReadyToSpawnEnemy { get; set; }

        Timer coinSpawner;
        Timer enemySpawner;

        public int placedCoins;
        public int currentCoins;
        public int coinsToSpawn;

        public GameController(MonoJam mjIn)
        {
            mj = mjIn;

            player = new Player();
            enemies = new Enemy[MAX_ENEMIES];

            ResetCoinData();

            coins = new List<Coin>();
            random = new Random();

            coinSpawner = new Timer(1);
            coinSpawner.Elapsed += (a, b) => ReadyToSpawnCoins = true;
            coinSpawner.Start();

            enemySpawner = new Timer(10);
            enemySpawner.Elapsed += (a, b) => ReadyToSpawnEnemy = true;
            enemySpawner.Start();

            currentCoins = 0;
            coinsToSpawn = 0;
            placedCoins = 0;
            placedCoins = 0;
        }

        private void ResetCoinData()
        {
            coinData = new byte[MonoJam.WINDOW_WIDTH * MonoJam.WINDOW_HEIGHT];
        }

        public void Update()
        {
            // Always keep console window clear.
            Console.Clear();

            player.Update();

            for(int i = 0; i < totalEnemies; i++)
            {
                enemies[i].Update();
            }

            if(Keyboard.GetState().IsKeyDown(Keys.C))
            {
                AddCoins(2);
            }

            Console.WriteLine($"COINS: {currentCoins}; to spawn: {coinsToSpawn} (on screen: {coins.Count})");

            // Remove destroyed coins.
            for(int i = coins.Count - 1; i >= 0; i--)
            {
                for(int m = 0; m < coins[i].fallBy; m++)
                {
                    coins[i].MoveBy(new Vector2(0, 1));

                    if (coins[i].MoveAndCheckLand(coinData))
                    {
                        var pos = coins[i].CollisionRect.Location;
                        int arrayLoc = pos.Y * MonoJam.WINDOW_WIDTH + pos.X;

                        var coinArray = Enumerable.Repeat((byte)1, Coin.COIN_WIDTH).ToArray();

                        Buffer.BlockCopy(coinArray, 0, coinData, arrayLoc, coinArray.Length);

                        coins.RemoveAt(i);

                        placedCoins++;
                        break;
                    }
                }

                // Move coin buffers if required.
                if(placedCoins > 5000)
                {
                    placedCoins = 0;
                    mj.grc.CreateNewCoinBuffer();
                    ResetCoinData();
                }
            }

            // Remove destroyed enemies.
            for(int i = 0; i < totalEnemies; i++)
            {
                if(enemies[i].ReadyToRemove)
                {
                    DestroyEnemy(enemies[i]);
                }
            }

            if(ReadyToSpawnCoins)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (coinsToSpawn > 0)
                    {
                        coinsToSpawn--;

                        // TODO: "Trend" coins into piles
                        var newCoin = new Coin();
                        newCoin.SetX(random.Next(0, MonoJam.WINDOW_WIDTH - Coin.COIN_WIDTH + 1));
                        coins.Add(newCoin);

                        ReadyToSpawnCoins = false;
                    }
                }
            }
            
            if(ReadyToSpawnEnemy)
            {
                SpawnEnemy();

                ReadyToSpawnEnemy = false;
            }
        }

        public void AddCoins(int coins)
        {
            currentCoins += coins;
            coinsToSpawn += coins;
        }

        public void SpawnEnemy()
        {
            if(totalEnemies < MAX_ENEMIES)
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
