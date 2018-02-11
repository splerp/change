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
        private MonoJam mj;

        public Player player;
        public Enemy[] enemies;
        public List<Coin> coins;

        public byte[] coinData;

        public static Random random;
        public bool ReadyToSpawnCoins { get; set; }

        Timer coinSpawner;

        public int placedCoins;
        public int currentCoins;
        public int coinsToSpawn;

        public GameController(MonoJam mjIn)
        {
            mj = mjIn;

            player = new Player();

            ResetCoinData();

            coins = new List<Coin>();
            random = new Random();

            coinSpawner = new Timer(1);
            coinSpawner.Elapsed += (a, b) => ReadyToSpawnCoins = true;
            coinSpawner.Start();

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
        }

        public void AddCoins(int coins)
        {
            currentCoins += coins;
            coinsToSpawn += coins;
        }
    }
}
