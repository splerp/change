using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace MonoJam
{
    class GameController
    {
        private MonoJam mj;

        public Player player;
        public Enemy[] enemies;
        public List<Coin> coins;

        public byte[] coinData;

        public Random random;
        public bool ReadyToSpawnCoins { get; set; }

        Timer coinSpawner;

        public int currentCoins;
        public int coinsToSpawn;

        public GameController(MonoJam mjIn)
        {
            mj = mjIn;

            player = new Player();
            coinData = new byte[MonoJam.WINDOW_WIDTH * MonoJam.WINDOW_HEIGHT];

            coins = new List<Coin>();
            random = new Random();

            coinSpawner = new Timer(1);
            coinSpawner.Elapsed += (a, b) => ReadyToSpawnCoins = true;
            coinSpawner.Start();

            currentCoins = 0;
            coinsToSpawn = 0;
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

                    if (coins[i].MoveCheckLand(coinData))
                    {
                        var pos = coins[i].CollisionRect.Location;
                        int arrayLoc = pos.Y * MonoJam.WINDOW_WIDTH + pos.X;

                        var coinArray = Enumerable.Repeat((byte)1, Coin.COIN_WIDTH).ToArray();

                        Buffer.BlockCopy(coinArray, 0, coinData, arrayLoc, coinArray.Length);

                        coins.RemoveAt(i);
                        break;
                    }
                }
            }

            if(ReadyToSpawnCoins)
            {
                if (coinsToSpawn > 0)
                {
                    coinsToSpawn--;

                    var newCoin = new Coin();
                    newCoin.SetX(random.Next(0, MonoJam.WINDOW_WIDTH - Coin.COIN_WIDTH + 1));

                    coins.Add(newCoin);

                    ReadyToSpawnCoins = false;
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
