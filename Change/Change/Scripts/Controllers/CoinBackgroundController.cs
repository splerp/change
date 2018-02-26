using Microsoft.Xna.Framework;
using Splerp.Change.Events;
using Splerp.Change.GameObjects;
using Splerp.Change.Utils;
using System;
using System.Collections.Generic;

namespace Splerp.Change.Controllers
{
    public sealed class CoinBackgroundController
    {
        public const int COINS_PER_LAYER = 5000;
        public const int COINS_SPAWNED_PER_FRAME = 8;

        // Keep track of how many coins are waiting to be created.
        public Int64 coinsToSpawn;

        // Keep track of how many coins are in the current layer.
        private Int64 coinsInLayer;

        // The list of currently falling coins.
        public List<Coin> coins;

        // Array of placed coin data (where 1 = coin placed, 0 = empty space).
        private byte[] coinDataBuffer;

        // Defines the coin "trend". As more coins spawn, piles of coins should
        // start to become apparent. The coin trend array is used while
        // generating coins to produce this effect.
        private int[] currentLayerCoinTrend;
        private int totalTrendCount;
        
        // Custom events for use in other classes.
        public static event EventHandler<EventArgs> CoinBufferCompleted;
        public static event EventHandler<CoinBufferUpdatedArgs> CurrentCoinBufferUpdated;
        public static event EventHandler<CoinBufferClearedArgs> CurrentCoinBufferCleared;

        public CoinBackgroundController()
        {
            // Initialise arrays and lists.
            coins = new List<Coin>();
            coinDataBuffer = new byte[ChangeGame.PLAYABLE_AREA_WIDTH * ChangeGame.PLAYABLE_AREA_HEIGHT];
            currentLayerCoinTrend = new int[ChangeGame.PLAYABLE_AREA_WIDTH];
        }
        
        // Reset all variables and clear background buffer.
        public void Reset()
        {
            coinsToSpawn = 0;
            coinsInLayer = 0;

            coins.Clear();

            CalculateCoinTrend();
            ClearCoinDataBuffer();
        }

        public void ClearCoinDataBuffer()
        {
            Array.Clear(coinDataBuffer, 0, ChangeGame.PLAYABLE_AREA_WIDTH * ChangeGame.PLAYABLE_AREA_HEIGHT);
            CurrentCoinBufferCleared(this, new CoinBufferClearedArgs(coinDataBuffer));
        }

        // Determines when coins should be spawned, updated, and moved into the background.
        public void Update(GameTime gameTime)
        {
            if (coinsToSpawn > 0)
            {
                SoundController.Play(Sound.CoinsDrop, true);
            }
            else
            {
                SoundController.Stop(Sound.CoinsDrop);
            }

            for (int i = 0; i < COINS_SPAWNED_PER_FRAME; i++)
            {
                if (coinsToSpawn > 0)
                {
                    coinsToSpawn--;

                    var randomNum = GameController.random.Next(0, totalTrendCount);
                    int j = 0;
                    while (randomNum > 0)
                    {
                        randomNum -= currentLayerCoinTrend[j];
                        j++;
                    }

                    var newCoin = new Coin();
                    newCoin.SetX(j);
                    coins.Add(newCoin);
                }
            }
            
            // Update and remove destroyed coins.
            for (int i = coins.Count - 1; i >= 0; i--)
            {
                var moveBy = coins[i].fallBy * (float)gameTime.ElapsedGameTime.TotalSeconds;

                for (int m = 0; m < moveBy; m++)
                {
                    coins[i].MoveBy(new Vector2(0, 1));

                    if (coins[i].MoveAndCheckLand(coinDataBuffer))
                    {

                        var pos = coins[i].CollisionRect.Location;
                        int arrayLoc = pos.Y * ChangeGame.PLAYABLE_AREA_WIDTH + pos.X;

                        coinDataBuffer[arrayLoc] = 1;

                        coins.RemoveAt(i);

                        coinsInLayer++;

                        CurrentCoinBufferUpdated(this, new CoinBufferUpdatedArgs(arrayLoc));
                        break;
                    }
                }

                // Move coin buffers if required.
                if (coinsInLayer >= COINS_PER_LAYER)
                {
                    coinsInLayer = 0;
                    CalculateCoinTrend();

                    CoinBufferCompleted(this, EventArgs.Empty);

                    ClearCoinDataBuffer();
                }
            }
        }

        // Use perlin noise to generate a trend array that creates piles of money.
        public void CalculateCoinTrend()
        {
            float randomStartPos = GameController.random.Next(0, 100) / 100f;
            totalTrendCount = 0;
            for (int i = 0; i < currentLayerCoinTrend.Length; i++)
            {
                float newVal = 5 + (Perlin.Noise(0, i * 0.05f + randomStartPos) * 5) + 1;
                currentLayerCoinTrend[i] = (int)newVal;
                totalTrendCount += (int)newVal;
            }
        }
    }
}
