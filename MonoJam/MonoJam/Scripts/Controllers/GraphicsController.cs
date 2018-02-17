using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoJam.GameObjects;
using MonoJam.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoJam.Controllers
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
        private Texture2D paddleGraphic;
        private Texture2D coinGraphic;
        private Texture2D VaultWalls;
        private Texture2D VaultFloor;
        private Texture2D piggyBankGraphic;
        private Texture2D noteGraphic;
        private Texture2D[] enemyFireGraphics;
        private Texture2D[] noteFireGraphics;
        private Texture2D hudBackground;
        private Texture2D hudLaserCharge;
        private Texture2D hudPlayerHealth;
        private Texture2D backgroundGraphic;
        private Texture2D titleGraphic;
        private Texture2D titleCursor;
        private Texture2D gameOverBackground;
        private Texture2D paddleBackground;

        private List<CoinBackgroundLayer> coinBackgroundLayers;
        private Texture2D currentCoinBackground;
        #endregion

        private const int vaultWallWidth = 10;
        private const int vaultFloorHeight = 20;
        private Point gameOverOffset = new Point(0, -6);

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

        public void ResetCoinBuffers()
        {
            coinBackgroundLayers.Clear();
            coinBackgroundLayers.Add(new CoinBackgroundLayer() { graphic = backgroundGraphic });
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

            coinGraphic = new Texture2D(graphicsDevice, 1, 1);
            coinGraphic.SetData(new Color[] { Color.Yellow });
            
            backgroundGraphic = Content.Load<Texture2D>("Graphics/Background");
            titleGraphic = Content.Load<Texture2D>("Graphics/Title");
            titleCursor = Content.Load<Texture2D>("Graphics/RoundCoin");
            gameOverBackground = Content.Load<Texture2D>("Graphics/GameOver");

            currentCoinBackground = new Texture2D(graphicsDevice, MonoJam.PLAYABLE_AREA_WIDTH, MonoJam.PLAYABLE_AREA_HEIGHT);
            VaultWalls = new Texture2D(graphicsDevice, vaultWallWidth, MonoJam.PLAYABLE_AREA_HEIGHT);
            VaultFloor = new Texture2D(graphicsDevice, MonoJam.PLAYABLE_AREA_WIDTH + vaultWallWidth * 2, vaultFloorHeight);
            hudBackground = new Texture2D(graphicsDevice, MonoJam.PLAYABLE_AREA_WIDTH, MonoJam.HUD_HEIGHT);
            hudLaserCharge = new Texture2D(graphicsDevice, MonoJam.PLAYABLE_AREA_WIDTH, 2);
            hudPlayerHealth = new Texture2D(graphicsDevice, MonoJam.PLAYABLE_AREA_WIDTH, 3);
            paddleGraphic = new Texture2D(graphicsDevice, PaddlePlayer.WIDTH, PaddlePlayer.HEIGHT);
            paddleBackground = new Texture2D(graphicsDevice, MonoJam.WINDOW_WIDTH, MonoJam.PADDLE_AREA_HEIGHT);

            piggyBankGraphic = Content.Load<Texture2D>("Graphics/Pig");
            enemyFireGraphics = new Texture2D[]
            {
                Content.Load<Texture2D>("Graphics/Fire2"),
                Content.Load<Texture2D>("Graphics/Fire1"),
                Content.Load<Texture2D>("Graphics/Fire3"),
                Content.Load<Texture2D>("Graphics/Fire1"),
            };

            noteGraphic = Content.Load<Texture2D>("Graphics/5Dollars");
            noteFireGraphics = new Texture2D[]
            {
                Content.Load<Texture2D>("Graphics/FireMoney1"),
                Content.Load<Texture2D>("Graphics/FireMoney2"),
                Content.Load<Texture2D>("Graphics/FireMoney3"),
                Content.Load<Texture2D>("Graphics/FireMoney4"),
                Content.Load<Texture2D>("Graphics/FireMoney5"),
                Content.Load<Texture2D>("Graphics/FireMoney6"),
            };

            VaultWalls.SetData(Enumerable.Repeat(new Color(59, 33, 12), VaultWalls.Width * VaultWalls.Height).ToArray());
            VaultFloor.SetData(Enumerable.Repeat(new Color(36, 17, 1), VaultFloor.Width * VaultFloor.Height).ToArray());
            hudBackground.SetData(Enumerable.Repeat(Color.Black, hudBackground.Width * hudBackground.Height).ToArray());
            hudLaserCharge.SetData(Enumerable.Repeat(Color.Red, hudLaserCharge.Width * hudLaserCharge.Height).ToArray());
            paddleGraphic.SetData(Enumerable.Repeat(Color.White, paddleGraphic.Width * paddleGraphic.Height).ToArray());
            paddleBackground.SetData(Enumerable.Repeat(Color.Black, paddleBackground.Width * paddleBackground.Height).ToArray());

            var hpBar = Enumerable.Repeat(Color.Green, hudPlayerHealth.Width * (hudPlayerHealth.Height - 1))
                .Concat(Enumerable.Repeat(Color.DarkGreen, hudPlayerHealth.Width * 1))
                .ToArray();
            hudPlayerHealth.SetData(hpBar);
        }

        // TODO: Just set the relevant pixels when required, not a full refresh.
        private void UpdateCoinBGData()
        {
            var data = gc.coinData.Select(c => c == 0 ? Color.Transparent : Color.Yellow).ToArray();
            currentCoinBackground.SetData(data);
        }

        public void Draw()
        {
            switch(gc.currentState)
            {
                case GameController.GameState.Title:
                    DrawMenu();
                    break;
                case GameController.GameState.Playing:
                    DrawGame();
                    break;
                case GameController.GameState.GameOver:
                    DrawGame();
                    DrawGameOverMenu();
                    break;
            }
        }

        public void DrawMenu()
        {
            Vector2 coinPos = Vector2.Zero;
            switch (gc.mainMenu.selectedOption)
            {
                case 0:
                    coinPos = new Vector2(9, 7);
                    break;
                case 1:
                    coinPos = new Vector2(9, 20);
                    break;
                case 2:
                    coinPos = new Vector2(9, 35);
                    break;
            }

            batch.Begin(samplerState: samplerState, transformMatrix: baseScaleMatrix);
            {
                batch.Draw(titleGraphic, Vector2.Zero, Color.White);
                batch.Draw(titleCursor, coinPos, Color.White);
            }
            batch.End();
        }

        public void DrawGameOverMenu()
        {
            Vector2 coinPos = Vector2.Zero;
            switch (gc.gameOverMenu.selectedOption)
            {
                case 0:
                    coinPos = new Vector2(33, 30);
                    break;
                case 1:
                    coinPos = new Vector2(34, 46);
                    break;
            }

            var menuPos = (gameOverOffset + new Point(0, (int)gc.gameOverMenu.currentY)).ToVector2();

            batch.Begin(samplerState: samplerState, transformMatrix: baseScaleMatrix);
            {
                batch.Draw(gameOverBackground, menuPos, Color.White);
                batch.Draw(titleCursor, coinPos + menuPos, Color.White);
            }
            batch.End();
        }

        public void DrawGame()
        {
            var baseMatrixWithMainShake =
                Matrix.CreateTranslation(new Vector3(gc.mainShaker.CurrentShake, 0))
                * baseMatrix;

            var baseMatrixWithLaserShake = Matrix.CreateTranslation(
                new Vector3(gc.laserPlayer.laserShake.CurrentShake, 0))
                * baseMatrixWithMainShake;

            graphicsDevice.Clear(Color.Black);

            for (int i = coinBackgroundLayers.Count - 1; i >= 0; i--)
            {
                var coinBackground = coinBackgroundLayers[i];

                // Set to values immediately when start of the game (no smoothing should be applied).
                var lerpSpeed = coinBackgroundLayers.Count == 1 ? 1f : 0.01f;

                float targetScale = 1 * (float)Math.Pow(0.9f, (i + 1));
                float targetTranslate = 300 - (300 * (float)Math.Pow(0.9f, (i + 1)));
                float targetAlpha = 0.75f - ((i / (float)MAX_VAULT_BG_LAYERS_VISIBLE) * 0.75f);

                // Move each background layer towards target values.
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
                    batch.Draw(VaultFloor, new Vector2(-vaultWallWidth, -vaultFloorHeight), drawColour);
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
                    var oldEnemySize = new Vector2(10, 10);
                    var enemyDiff = new Vector2(piggyBankGraphic.Width, piggyBankGraphic.Height) - oldEnemySize;


                    var fireOffset = new Vector2(-1, -5);

                    var effect = c.speed.X < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                    var effectInv = c.speed.X > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                    batch.Draw(enemyFireGraphics[c.animationFrame % enemyFireGraphics.Length],
                        (c.Position + fireOffset).ToPoint().ToVector2(),
                        null,
                        Color.White,
                        0, Vector2.Zero, 1, effect, 0);

                    batch.Draw(piggyBankGraphic, c.Position.ToPoint().ToVector2(),
                        null,
                        Color.White,
                        0, Vector2.Zero, 1, effectInv, 0);
                }
                for (int i = 0; i < gc.totalEnemies; i++)
                {
                    var effect = gc.enemies[i].direction < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                    var effectInv = gc.enemies[i].direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                    batch.Draw(piggyBankGraphic, gc.enemies[i].CollisionRect.Location.ToVector2(),
                        null,
                        Color.White,
                        0, Vector2.Zero, 1, effectInv, 0);
                }
            }
            batch.End();

            // Draw money.
            batch.Begin(samplerState: samplerState, transformMatrix: baseMatrixWithMainShake);
            {
                foreach (var c in gc.notesOnFire)
                {
                    var fireOffset = new Vector2(-1, -(NoteOnFire.HEIGHT - Note.HEIGHT));

                    var effect = c.IsFlipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                    batch.Draw(noteFireGraphics[c.animationFrame % noteFireGraphics.Length],
                        (c.Position + fireOffset).ToPoint().ToVector2(),
                        null,
                        Color.White,
                        0, Vector2.Zero, 1, effect, 0);
                }
                foreach (var note in gc.notes)
                {
                    batch.Draw(noteGraphic, note.CollisionRect.Location.ToVector2(), Color.White);
                }
            }
            batch.End();

            // Draw player.
            batch.Begin(samplerState: samplerState, transformMatrix: baseMatrixWithLaserShake);
            {
                batch.Draw(playerGraphic, gc.laserPlayer.CollisionRect.Location.ToVector2(), Color.White);
            }
            batch.End();

            batch.Begin(samplerState: samplerState, transformMatrix: baseScaleMatrix);
            {
                batch.Draw(paddleBackground, new Vector2(0, MonoJam.WINDOW_HEIGHT - MonoJam.PADDLE_AREA_HEIGHT), Color.White);
            }
            batch.End();

            batch.Begin(samplerState: samplerState, transformMatrix: baseMatrix);
            {
                batch.Draw(paddleGraphic, gc.paddlePlayer.CollisionRect.Location.ToVector2(), Color.White);
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

            // TODO: Combine both sets of data, add to texture2D, draw once.
            if (gc.laserPlayer.FiringLaser)
            {
                var laserAlpha = Math.Min(gc.laserPlayer.laserCharge * 8, 1f);
                Color laserFadeColor = new Color(1f, 1f, 1f, laserAlpha);

                if (mousePos.X >= 0 && mousePos.Y >= 0 &&
                    mousePos.X < MonoJam.PLAYABLE_AREA_WIDTH && mousePos.Y < MonoJam.PLAYABLE_AREA_HEIGHT)
                {

                    var startPos = gc.laserPlayer.LeftEyePos;
                    var newData = LineGraphic.CreateLine(startPos.X, startPos.Y, mousePos.X, mousePos.Y, Color.Red);
                    playerLasersLayer.SetData(newData);

                    batch.Begin(samplerState: samplerState, transformMatrix: baseMatrixWithLaserShake, blendState: BlendState.NonPremultiplied);
                    {
                        batch.Draw(playerLasersLayer, Vector2.Zero, laserFadeColor);
                    }
                    batch.End();

                    startPos = gc.laserPlayer.RightEyePos;
                    newData = LineGraphic.CreateLine(startPos.X, startPos.Y, mousePos.X, mousePos.Y, Color.Red);
                    playerLasersLayer.SetData(newData);

                    batch.Begin(samplerState: samplerState, transformMatrix: baseMatrixWithLaserShake, blendState: BlendState.NonPremultiplied);
                    {
                        batch.Draw(playerLasersLayer, Vector2.Zero, laserFadeColor);
                    }
                    batch.End();
                }
            }

            // Draw hud.
            var laserPercentage = (int)(gc.laserPlayer.laserCharge * MonoJam.PLAYABLE_AREA_WIDTH) / (float)MonoJam.PLAYABLE_AREA_WIDTH;
            var healthPercentage = (GameController.MAX_NOTES_MISSED - gc.notesMissed) / (float)GameController.MAX_NOTES_MISSED;

            batch.Begin(samplerState: samplerState, transformMatrix: baseScaleMatrix);
            {
                batch.Draw(hudBackground, Vector2.Zero, Color.White);

                batch.Draw(hudLaserCharge,
                    new Vector2(0, 3),
                    null,
                    Color.White,
                    0, Vector2.Zero,
                    new Vector2(laserPercentage, 1),
                    SpriteEffects.None, 0);

                batch.Draw(hudPlayerHealth,
                    new Vector2(0, 0),
                    null,
                    Color.White,
                    0, Vector2.Zero,
                    new Vector2(healthPercentage, 1),
                    SpriteEffects.None, 0);
            }
            batch.End();
        }
    }
}
