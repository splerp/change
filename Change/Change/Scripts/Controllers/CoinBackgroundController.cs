using Microsoft.Xna.Framework;
using MonoJam.GameObjects;
using MonoJam.Utils;
using System;
using System.Collections.Generic;

namespace MonoJam.Controllers
{
    public class CoinBackgroundController
    {
        public const int COINS_PER_LAYER = 5000;
        public const int COINS_SPAWNED_PER_FRAME = 8;

        public byte[] coinDataBuffer;
        public int[] currentLayerCoinTrend;
        public int totalTrendCount;

        public Int64 placedCoins;
        public Int64 coinsToSpawn;

        public List<Coin> coins;
        
        public static event EventHandler<EventArgs> CoinBufferCompleted;
        public static event EventHandler<EventArgs> CurrentCoinBufferUpdated;

        public CoinBackgroundController()
        {
            currentLayerCoinTrend = new int[MonoJam.PLAYABLE_AREA_WIDTH];

            coins = new List<Coin>();
            coinDataBuffer = new byte[MonoJam.PLAYABLE_AREA_WIDTH * MonoJam.PLAYABLE_AREA_HEIGHT];
        }
        
        public void Reset()
        {
            coinsToSpawn = 0;
            placedCoins = 0;

            coins.Clear();

            CalculateCoinTrend();
            ClearCoinDataBuffer();
        }

        public void ClearCoinDataBuffer()
        {
            Array.Clear(coinDataBuffer, 0, MonoJam.PLAYABLE_AREA_WIDTH * MonoJam.PLAYABLE_AREA_HEIGHT);
            CurrentCoinBufferUpdated(this, EventArgs.Empty);
        }

        public void Update()
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


            // Remove destroyed coins.
            for (int i = coins.Count - 1; i >= 0; i--)
            {
                for (int m = 0; m < coins[i].fallBy; m++)
                {
                    coins[i].MoveBy(new Vector2(0, 1));

                    if (coins[i].MoveAndCheckLand(coinDataBuffer))
                    {

                        var pos = coins[i].CollisionRect.Location;
                        int arrayLoc = pos.Y * MonoJam.PLAYABLE_AREA_WIDTH + pos.X;

                        coinDataBuffer[arrayLoc] = 1;

                        coins.RemoveAt(i);

                        placedCoins++;

                        CurrentCoinBufferUpdated(this, EventArgs.Empty);
                        break;
                    }
                }

                // Move coin buffers if required.
                if (placedCoins >= COINS_PER_LAYER)
                {
                    placedCoins = 0;
                    CalculateCoinTrend();

                    CoinBufferCompleted(this, EventArgs.Empty);

                    ClearCoinDataBuffer();
                }
            }
        }

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
