using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoJam
{
    public class GraphicsController
    {
        public const int MAX_VAULT_BG_LAYERS_VISIBLE = 10;
        public const int MAX_VAULT_BG_LAYERS = 13;

        private GameController gc;
        private GraphicsDevice graphicsDevice;
        private SpriteBatch batch;
        private SamplerState samplerState;
        private Matrix baseScaleMatrix;

        #region Game graphics
        private Texture2D playerGraphic;
        private Texture2D playerLasersLayer;
        private Texture2D coinGraphic;
        private Texture2D VaultWalls;
        private Texture2D VaultFloor;
        private Texture2D enemyGraphic;

        private List<CoinBackgroundLayer> coinBackgroundLayers;
        private Texture2D currentCoinBackground;
        #endregion

        private const int vaultWallWidth = 10;

        public GraphicsController(GameController gcIn, GraphicsDevice graphicsDeviceIn)
        {
            gc = gcIn;
            graphicsDevice = graphicsDeviceIn;
            coinBackgroundLayers = new List<CoinBackgroundLayer>();

            batch = new SpriteBatch(graphicsDevice);
            samplerState = new SamplerState() { Filter = TextureFilter.Point };
            baseScaleMatrix = Matrix.CreateScale(MonoJam.SCALE);
        }

        public void CreateNewCoinBuffer()
        {
            // First ensure graphic is up to date.
            UpdateCoinBGData();
            
            Color[] theCurrentData = new Color[MonoJam.WINDOW_WIDTH * MonoJam.WINDOW_HEIGHT];

            var NewCoinBackground = new CoinBackgroundLayer
            {
                graphic = new Texture2D(graphicsDevice, MonoJam.WINDOW_WIDTH, MonoJam.WINDOW_HEIGHT)
            };
            
            // Put data into background.
            currentCoinBackground.GetData(theCurrentData);
            NewCoinBackground.graphic.SetData(theCurrentData);

            coinBackgroundLayers = new List<CoinBackgroundLayer> { NewCoinBackground }.Concat(coinBackgroundLayers).ToList();

            // Forcefully remove any past this stage.
            if(coinBackgroundLayers.Count > MAX_VAULT_BG_LAYERS)
            {
                coinBackgroundLayers.RemoveAt(MAX_VAULT_BG_LAYERS);
            }
        }

        public void LoadContent(ContentManager Content)
        {
            playerGraphic = Content.Load<Texture2D>("Graphics/Player");
            playerLasersLayer = new Texture2D(graphicsDevice, MonoJam.WINDOW_WIDTH, MonoJam.WINDOW_HEIGHT);

            coinGraphic = new Texture2D(graphicsDevice, Coin.COIN_WIDTH, 1);
            coinGraphic.SetData(Enumerable.Repeat(Color.Yellow, Coin.COIN_WIDTH).ToArray());

            currentCoinBackground = new Texture2D(graphicsDevice, MonoJam.WINDOW_WIDTH, MonoJam.WINDOW_HEIGHT);
            VaultWalls = new Texture2D(graphicsDevice, vaultWallWidth, MonoJam.WINDOW_HEIGHT);
            VaultFloor = new Texture2D(graphicsDevice, MonoJam.WINDOW_WIDTH + vaultWallWidth * 2, 20);
            enemyGraphic = new Texture2D(graphicsDevice, Enemy.WIDTH, Enemy.HEIGHT);

            VaultWalls.SetData(Enumerable.Repeat(Color.Brown, VaultWalls.Width * VaultWalls.Height).ToArray());
            VaultFloor.SetData(Enumerable.Repeat(Color.DarkSlateGray, VaultFloor.Width * VaultFloor.Height).ToArray());
            enemyGraphic.SetData(Enumerable.Repeat(Color.DarkSlateGray, enemyGraphic.Width * enemyGraphic.Height).ToArray());
        }

        // TODO: Just set the relevant pixels when required, not a full refresh.
        private void UpdateCoinBGData()
        {
            var data = gc.coinData.Select(c => c == 0 ? Color.Transparent : Color.Yellow).ToArray();
            currentCoinBackground.SetData(data);
        }

        public void Draw()
        {
            graphicsDevice.Clear(Color.Black);

            for (int i = coinBackgroundLayers.Count - 1; i >= 0; i--)
            {
                var coinBackground = coinBackgroundLayers[i];

                // Move each background layer towards target values.
                var lerpSpeed = 0.01f;
                float targetScale = 1 * (float)Math.Pow(0.9f, (i + 1));
                float targetTranslate = 300 - (300 * (float)Math.Pow(0.9f, (i + 1)));
                float targetAlpha = 0.75f - ((i / (float)MAX_VAULT_BG_LAYERS_VISIBLE) * 0.75f);
                coinBackground.currentScale = MathHelper.Lerp(coinBackground.currentScale, targetScale, lerpSpeed);
                coinBackground.currentTranslate = MathHelper.Lerp(coinBackground.currentTranslate, targetTranslate, lerpSpeed);
                coinBackground.currentAlpha = MathHelper.Lerp(coinBackground.currentAlpha, targetAlpha, lerpSpeed);

                var drawColour = new Color(
                    coinBackground.currentAlpha,
                    coinBackground.currentAlpha,
                    coinBackground.currentAlpha, 1f);

                // Create matrix for coin backgrounds.
                Matrix coinBackgroundMatrix =
                      Matrix.CreateTranslation(-MonoJam.WINDOW_WIDTH / 2, -MonoJam.WINDOW_HEIGHT, 0)
                    * baseScaleMatrix
                    * Matrix.CreateScale(coinBackground.currentScale)
                    * Matrix.CreateTranslation(
                        MonoJam.WINDOW_WIDTH * MonoJam.SCALE / 2,
                        MonoJam.WINDOW_HEIGHT * MonoJam.SCALE - coinBackground.currentTranslate, 0);

                // Draw buffer coin background.
                batch.Begin(
                    sortMode: SpriteSortMode.BackToFront,
                    samplerState: samplerState,
                    transformMatrix: coinBackgroundMatrix);
                {
                    batch.Draw(coinBackground.graphic, new Vector2(0, 0), drawColour);
                    batch.Draw(VaultWalls, new Vector2(-vaultWallWidth, 0), drawColour);
                    batch.Draw(VaultWalls, new Vector2(MonoJam.WINDOW_WIDTH, 0), drawColour);
                    batch.Draw(VaultFloor, new Vector2(-vaultWallWidth, MonoJam.WINDOW_HEIGHT), drawColour);
                }
                batch.End();
            }
            
            // Update current coin data.
            UpdateCoinBGData();
            batch.Begin(samplerState: samplerState, transformMatrix: baseScaleMatrix);
            {
                batch.Draw(currentCoinBackground, new Vector2(0, 0), Color.White);
            }
            batch.End();

            // Draw enemies.
            batch.Begin(samplerState: samplerState, transformMatrix: baseScaleMatrix);
            {
                for (int i = 0; i < gc.totalEnemies; i++)
                {
                    batch.Draw(enemyGraphic, gc.enemies[i].CollisionRect.Location.ToVector2(), Color.White);
                }
            }
            batch.End();

            // Draw player.
            batch.Begin(samplerState: samplerState, transformMatrix: baseScaleMatrix);
            {
                batch.Draw(playerGraphic, gc.player.CollisionRect.Location.ToVector2(), Color.White);

                // Draw coins.
                foreach (var coin in gc.coins)
                {
                    batch.Draw(coinGraphic, coin.CollisionRect.Location.ToVector2(), Color.White);
                }
            }
            batch.End();

            var mousePos = Mouse.GetState().Position / new Point(MonoJam.SCALE);
            var playerPos = gc.player.CollisionRect.Location;

            // TODO: Combine both sets of data, add to texture2D, draw once.
            if (gc.player.FiringLaser)
            {
                if (mousePos.X >= 0 && mousePos.Y >= 0 &&
                    mousePos.X < MonoJam.WINDOW_WIDTH && mousePos.Y < MonoJam.WINDOW_HEIGHT)
                {
                    var newData = LineGraphic.CreateLine(playerPos.X + 2, playerPos.Y + 2, mousePos.X, mousePos.Y, Color.Red);
                    playerLasersLayer.SetData(newData);

                    batch.Begin(samplerState: samplerState, transformMatrix: baseScaleMatrix);
                    {
                        batch.Draw(playerLasersLayer, Vector2.Zero, Color.White);
                    }
                    batch.End();

                    newData = LineGraphic.CreateLine(playerPos.X + 5, playerPos.Y + 2, mousePos.X, mousePos.Y, Color.Red);
                    playerLasersLayer.SetData(newData);

                    batch.Begin(samplerState: samplerState, transformMatrix: baseScaleMatrix);
                    {
                        batch.Draw(playerLasersLayer, Vector2.Zero, Color.White);
                    }
                    batch.End();
                }
            }
        }
    }
}
