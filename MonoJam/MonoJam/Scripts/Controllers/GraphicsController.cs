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
        private Matrix baseMatrix;
        private Matrix baseScaleMatrix;

        #region Game graphics
        private Texture2D playerGraphic;
        private Texture2D playerLasersLayer;
        private Texture2D coinGraphic;
        private Texture2D VaultWalls;
        private Texture2D VaultFloor;
        private Texture2D enemyGraphic;
        private Texture2D[] enemyFireGraphics;
        private Texture2D hudLaserCharge;

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
            baseMatrix =
                  Matrix.CreateTranslation(0, MonoJam.PLAYABLE_AREA_Y, 0)
                * baseScaleMatrix;
        }

        public void CreateNewCoinBuffer()
        {
            // First ensure graphic is up to date.
            UpdateCoinBGData();
            
            Color[] theCurrentData = new Color[MonoJam.PLAYABLE_AREA_WIDTH * MonoJam.PLAYABLE_AREA_HEIGHT];

            var NewCoinBackground = new CoinBackgroundLayer
            {
                graphic = new Texture2D(graphicsDevice, MonoJam.PLAYABLE_AREA_WIDTH, MonoJam.PLAYABLE_AREA_HEIGHT)
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
            playerLasersLayer = new Texture2D(graphicsDevice, MonoJam.PLAYABLE_AREA_WIDTH, MonoJam.PLAYABLE_AREA_HEIGHT);

            coinGraphic = new Texture2D(graphicsDevice, Coin.COIN_WIDTH, 1);
            coinGraphic.SetData(Enumerable.Repeat(Color.Yellow, Coin.COIN_WIDTH).ToArray());

            currentCoinBackground = new Texture2D(graphicsDevice, MonoJam.PLAYABLE_AREA_WIDTH, MonoJam.PLAYABLE_AREA_HEIGHT);
            VaultWalls = new Texture2D(graphicsDevice, vaultWallWidth, MonoJam.PLAYABLE_AREA_HEIGHT);
            VaultFloor = new Texture2D(graphicsDevice, MonoJam.PLAYABLE_AREA_WIDTH + vaultWallWidth * 2, 20);
            hudLaserCharge = new Texture2D(graphicsDevice, MonoJam.PLAYABLE_AREA_WIDTH, 5);

            enemyGraphic = Content.Load<Texture2D>("Graphics/Enemy");
            enemyFireGraphics = new Texture2D[]
            {
                Content.Load<Texture2D>("Graphics/Fire2"),
                Content.Load<Texture2D>("Graphics/Fire1"),
                Content.Load<Texture2D>("Graphics/Fire3"),
                Content.Load<Texture2D>("Graphics/Fire1"),
            };

            VaultWalls.SetData(Enumerable.Repeat(Color.Brown, VaultWalls.Width * VaultWalls.Height).ToArray());
            VaultFloor.SetData(Enumerable.Repeat(Color.DarkSlateGray, VaultFloor.Width * VaultFloor.Height).ToArray());
            hudLaserCharge.SetData(Enumerable.Repeat(Color.Red, hudLaserCharge.Width * hudLaserCharge.Height).ToArray());
        }

        // TODO: Just set the relevant pixels when required, not a full refresh.
        private void UpdateCoinBGData()
        {
            var data = gc.coinData.Select(c => c == 0 ? Color.Transparent : Color.Yellow).ToArray();
            currentCoinBackground.SetData(data);
        }

        public void Draw()
        {
            var baseMatrixWithMainShake = baseMatrix * Matrix.CreateTranslation(new Vector3(gc.mainShaker.CurrentShake, 0));
            var baseMatrixWithLaserShake = baseMatrixWithMainShake * Matrix.CreateTranslation(new Vector3(gc.player.laserShake.CurrentShake, 0));

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
                      Matrix.CreateTranslation(-MonoJam.WINDOW_WIDTH / 2, -MonoJam.PLAYABLE_AREA_HEIGHT, 0)
                    * baseMatrixWithMainShake
                    * Matrix.CreateScale(coinBackground.currentScale)
                    * Matrix.CreateTranslation(
                        MonoJam.PLAYABLE_AREA_WIDTH * MonoJam.SCALE / 2,
                        MonoJam.PLAYABLE_AREA_HEIGHT * MonoJam.SCALE - coinBackground.currentTranslate,
                        0);

                // Draw buffer coin background.
                batch.Begin(
                    sortMode: SpriteSortMode.BackToFront,
                    samplerState: samplerState,
                    transformMatrix: coinBackgroundMatrix);
                {
                    batch.Draw(coinBackground.graphic, new Vector2(0, 0), drawColour);
                    batch.Draw(VaultWalls, new Vector2(-vaultWallWidth, 0), drawColour);
                    batch.Draw(VaultWalls, new Vector2(MonoJam.PLAYABLE_AREA_WIDTH, 0), drawColour);
                    batch.Draw(VaultFloor, new Vector2(-vaultWallWidth, MonoJam.PLAYABLE_AREA_HEIGHT), drawColour);
                }
                batch.End();
            }
            
            // Update current coin data.
            UpdateCoinBGData();
            batch.Begin(samplerState: samplerState, transformMatrix: baseMatrixWithMainShake);
            {
                batch.Draw(currentCoinBackground, new Vector2(0, 0), Color.White);
            }
            batch.End();

            // Draw enemies.
            batch.Begin(samplerState: samplerState, transformMatrix: baseMatrixWithMainShake);
            {
                foreach (var c in gc.corpses)
                {
                    var fireOffset = new Vector2(-5, -5);

                    var effect = c.speed.X < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                    batch.Draw(enemyFireGraphics[c.animationFrame % enemyFireGraphics.Length],
                        (c.Position + fireOffset).ToPoint().ToVector2(),
                        null,
                        Color.White,
                        0, Vector2.Zero, 1, effect, 0);

                    batch.Draw(enemyGraphic, c.Position.ToPoint().ToVector2(), Color.White);
                }
                for (int i = 0; i < gc.totalEnemies; i++)
                {
                    batch.Draw(enemyGraphic, gc.enemies[i].CollisionRect.Location.ToVector2(), Color.White);
                }
            }
            batch.End();

            // Draw player.
            batch.Begin(samplerState: samplerState, transformMatrix: baseMatrixWithLaserShake);
            {
                batch.Draw(playerGraphic, gc.player.CollisionRect.Location.ToVector2(), Color.White);
            }
            batch.End();

            batch.Begin(samplerState: samplerState, transformMatrix: baseMatrixWithMainShake);
            {
                // Draw coins.
                foreach (var coin in gc.coins)
                {
                    batch.Draw(coinGraphic, coin.CollisionRect.Location.ToVector2(), Color.White);
                }
            }
            batch.End();

            var mousePos = Mouse.GetState().Position / new Point(MonoJam.SCALE) - new Point(0, MonoJam.PLAYABLE_AREA_Y);
            var playerPos = gc.player.CollisionRect.Location;

            // TODO: Combine both sets of data, add to texture2D, draw once.
            if (gc.player.FiringLaser)
            {
                if (mousePos.X >= 0 && mousePos.Y >= 0 &&
                    mousePos.X < MonoJam.PLAYABLE_AREA_WIDTH && mousePos.Y < MonoJam.PLAYABLE_AREA_HEIGHT)
                {
                    var newData = LineGraphic.CreateLine(playerPos.X + 2, playerPos.Y + 2, mousePos.X, mousePos.Y, Color.Red);
                    playerLasersLayer.SetData(newData);

                    batch.Begin(samplerState: samplerState, transformMatrix: baseMatrixWithLaserShake);
                    {
                        batch.Draw(playerLasersLayer, Vector2.Zero, Color.White);
                    }
                    batch.End();

                    newData = LineGraphic.CreateLine(playerPos.X + 5, playerPos.Y + 2, mousePos.X, mousePos.Y, Color.Red);
                    playerLasersLayer.SetData(newData);

                    batch.Begin(samplerState: samplerState, transformMatrix: baseMatrixWithLaserShake);
                    {
                        batch.Draw(playerLasersLayer, Vector2.Zero, Color.White);
                    }
                    batch.End();
                }
            }

            // Draw hud.
            var laserPercentage = (int)(gc.player.laserCharge * MonoJam.PLAYABLE_AREA_WIDTH) / (float)MonoJam.PLAYABLE_AREA_WIDTH;

            batch.Begin(samplerState: samplerState, transformMatrix: baseScaleMatrix);
            {
                batch.Draw(hudLaserCharge,
                    Vector2.Zero,
                    null,
                    Color.White,
                    0, Vector2.Zero,
                    new Vector2(laserPercentage, 1),
                    SpriteEffects.None, 0);
            }
            batch.End();
        }
    }
}
