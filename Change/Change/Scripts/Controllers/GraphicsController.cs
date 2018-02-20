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
        private Texture2D paddleGraphicFront;
        private Texture2D paddleGraphicBack;
        private Texture2D coinGraphic;
        private Texture2D VaultWalls;
        private Texture2D VaultFloor;
        private Texture2D piggyBankGraphic;
        private Texture2D vacuumBodyGraphic;
        private Texture2D vacuumHeadUpGraphic;
        private Texture2D vacuumHeadDownGraphic;

        private Texture2D noteGraphic5;
        private Texture2D noteGraphic10;
        private Texture2D noteGraphic20;
        private Texture2D noteGraphic50;
        private Texture2D noteGraphic100;
        private Texture2D[] enemyFireGraphics;
        private Texture2D[] noteFireGraphics;
        private Texture2D hudBackground;
        private Texture2D hudLaserCharge;
        private Texture2D hudPlayerHealth;
        private Texture2D hudTimeRemaining;
        private Texture2D backgroundGraphic;
        private Texture2D titleGraphic;
        private Texture2D titleBorderGraphic;
        private Texture2D titleCursor;
        private Texture2D bestScoreBackground;
        private Texture2D gameOverBackground;
        private Texture2D stageOverBackground;
        private Texture2D paddleBackground;
        private Texture2D scoreBackground;
        private Texture2D mutedIcon;
        private Texture2D mutedMusicIcon;
        private Texture2D noTutorialIcon;
        private Dictionary<char, Texture2D> fontGraphics;

        private List<CoinBackgroundLayer> coinBackgroundLayers;
        private Texture2D currentCoinBackground;
        #endregion

        private const int vaultWallWidth = 10;
        private const int vaultFloorHeight = 20;

        private Point gameOverOffset = new Point(0, -6);
        private Point stageCompleteOffset = new Point(0, -6);
        private Point laserPlayerOffset = new Point(-LaserPlayer.GRAPHIC_OUTER_WIDTH, -LaserPlayer.GRAPHIC_OUTER_WIDTH);
        private Point mutedIconOffset;
        private Point mutedMusicIconOffset;
        private Point noTutorialIconOffset;
        private Point bestScoreBackgroundOffset;

        private Dictionary<Note.NoteType, Color> noteColours = new Dictionary<Note.NoteType, Color>
        {
            { Note.NoteType.None, Color.White },
            { Note.NoteType.Pink5, new Color(207, 97, 216) },
            { Note.NoteType.Blue10, new Color(97, 151, 216) },
            { Note.NoteType.Red20,new Color(221, 83, 81) },
            { Note.NoteType.Yellow50, new Color(228, 152, 84) },
            { Note.NoteType.Green100, new Color(81, 231, 97) },
        };

        private Dictionary<Note.NoteType, Texture2D> noteGraphics;

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
            titleBorderGraphic = Content.Load<Texture2D>("Graphics/TitleBorder");
            titleCursor = Content.Load<Texture2D>("Graphics/RoundCoin");
            gameOverBackground = Content.Load<Texture2D>("Graphics/GameOver");
            stageOverBackground = Content.Load<Texture2D>("Graphics/StageOver");
            paddleBackground = Content.Load<Texture2D>("Graphics/PaddleTrack");
            paddleGraphicFront = Content.Load<Texture2D>("Graphics/PaddleFront");
            paddleGraphicBack = Content.Load<Texture2D>("Graphics/PaddleBack");
            scoreBackground = Content.Load<Texture2D>("Graphics/ScoreBackground");
            bestScoreBackground = Content.Load<Texture2D>("Graphics/BestScoreBackground");
            mutedIcon = Content.Load<Texture2D>("Graphics/MutedIcon");
            mutedMusicIcon = Content.Load<Texture2D>("Graphics/MutedMusicIcon");
            noTutorialIcon = Content.Load<Texture2D>("Graphics/NoTutorialIcon");

            bestScoreBackgroundOffset = new Point(MonoJam.WINDOW_WIDTH - 2, 2);
            mutedIconOffset = new Point(MonoJam.WINDOW_WIDTH - mutedIcon.Width - 3, MonoJam.WINDOW_HEIGHT - mutedIcon.Height - 3);
            mutedMusicIconOffset = new Point(MonoJam.WINDOW_WIDTH - mutedIcon.Width - 3, MonoJam.WINDOW_HEIGHT - mutedIcon.Height * 2 - 3 - 2);
            noTutorialIconOffset = new Point(MonoJam.WINDOW_WIDTH - noTutorialIcon.Width * 2 - 3 - 2, MonoJam.WINDOW_HEIGHT - noTutorialIcon.Height - 3);

            currentCoinBackground = new Texture2D(graphicsDevice, MonoJam.PLAYABLE_AREA_WIDTH, MonoJam.PLAYABLE_AREA_HEIGHT);
            VaultWalls = new Texture2D(graphicsDevice, vaultWallWidth, MonoJam.PLAYABLE_AREA_HEIGHT);
            VaultFloor = new Texture2D(graphicsDevice, MonoJam.PLAYABLE_AREA_WIDTH + vaultWallWidth * 2, vaultFloorHeight);
            hudBackground = new Texture2D(graphicsDevice, MonoJam.PLAYABLE_AREA_WIDTH, MonoJam.HUD_HEIGHT);
            hudLaserCharge = new Texture2D(graphicsDevice, MonoJam.PLAYABLE_AREA_WIDTH, 2);
            hudPlayerHealth = new Texture2D(graphicsDevice, MonoJam.PLAYABLE_AREA_WIDTH, 2);
            hudTimeRemaining = new Texture2D(graphicsDevice, MonoJam.PLAYABLE_AREA_WIDTH, 1);

            piggyBankGraphic = Content.Load<Texture2D>("Graphics/Pig");
            enemyFireGraphics = new Texture2D[]
            {
                Content.Load<Texture2D>("Graphics/Fire2"),
                Content.Load<Texture2D>("Graphics/Fire1"),
                Content.Load<Texture2D>("Graphics/Fire3"),
                Content.Load<Texture2D>("Graphics/Fire1"),
            };

            vacuumBodyGraphic = Content.Load<Texture2D>("Graphics/VacuumBody");
            vacuumHeadUpGraphic = Content.Load<Texture2D>("Graphics/VacuumHeadUp");
            vacuumHeadDownGraphic = Content.Load<Texture2D>("Graphics/VacuumHeadDown");

            noteGraphic5 = Content.Load<Texture2D>("Graphics/5Dollars");
            noteGraphic10 = Content.Load<Texture2D>("Graphics/10Dollars");
            noteGraphic20 = Content.Load<Texture2D>("Graphics/20Dollars");
            noteGraphic50 = Content.Load<Texture2D>("Graphics/50Dollars");
            noteGraphic100 = Content.Load<Texture2D>("Graphics/100Dollars");

            noteGraphics = new Dictionary<Note.NoteType, Texture2D>
            {
                { Note.NoteType.Pink5, noteGraphic5 },
                { Note.NoteType.Blue10, noteGraphic10 },
                { Note.NoteType.Red20, noteGraphic20 },
                { Note.NoteType.Yellow50, noteGraphic50 },
                { Note.NoteType.Green100, noteGraphic100 },
            };

            fontGraphics = new Dictionary<char, Texture2D>
            {
                { '0', Content.Load<Texture2D>("Graphics/Font/Font_0")},
                { '1', Content.Load<Texture2D>("Graphics/Font/Font_1")},
                { '2', Content.Load<Texture2D>("Graphics/Font/Font_2")},
                { '3', Content.Load<Texture2D>("Graphics/Font/Font_3")},
                { '4', Content.Load<Texture2D>("Graphics/Font/Font_4")},
                { '5', Content.Load<Texture2D>("Graphics/Font/Font_5")},
                { '6', Content.Load<Texture2D>("Graphics/Font/Font_6")},
                { '7', Content.Load<Texture2D>("Graphics/Font/Font_7")},
                { '8', Content.Load<Texture2D>("Graphics/Font/Font_8")},
                { '9', Content.Load<Texture2D>("Graphics/Font/Font_9")},
                { '.', Content.Load<Texture2D>("Graphics/Font/Font_Dot")},
            };

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

            var hpBar = Enumerable.Repeat(Color.Green, hudPlayerHealth.Width * (hudPlayerHealth.Height - 1))
                .Concat(Enumerable.Repeat(Color.DarkGreen, hudPlayerHealth.Width * 1))
                .ToArray();
            hudPlayerHealth.SetData(hpBar);

            hudTimeRemaining.SetData(Enumerable.Repeat(Color.Blue, hudTimeRemaining.Width * hudTimeRemaining.Height).ToArray());
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
                    DrawMainMenu();
                    break;
                case GameController.GameState.Playing:
                    DrawGame();
                    break;
                case GameController.GameState.BetweenStages:
                    DrawGame();
                    break;
                case GameController.GameState.GameOver:
                    DrawGame();
                    DrawGameOverMenu();
                    break;
            }
        }

        public void DrawMainMenu()
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

                var totalScoreLength = 11 + ScoreController.LengthOf(gc.bestCoinScore / 100d);
                var diff = bestScoreBackgroundOffset.X - totalScoreLength;

                if(gc.bestCoinScore > 0)
                {
                    batch.Draw(bestScoreBackground, bestScoreBackgroundOffset.ToVector2() - new Vector2(totalScoreLength, 0), Color.White);
                    DrawScore(batch, bestScoreBackgroundOffset.ToVector2() + new Vector2(11, 2) - new Vector2(totalScoreLength, 0), gc.bestCoinScore / 100d);
                }

                if (SoundController.Muted())
                {
                    batch.Draw(mutedIcon, mutedIconOffset.ToVector2(), Color.White);
                }

                if (SoundController.MusicMuted())
                {
                    batch.Draw(mutedMusicIcon, mutedMusicIconOffset.ToVector2(), Color.White);
                }
                if (gc.skipTutorial)
                {
                    batch.Draw(noTutorialIcon, noTutorialIconOffset.ToVector2(), Color.White);
                }
                
                batch.Draw(titleBorderGraphic, Vector2.Zero, Color.White);
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

            // Draw falling coins.
            batch.Begin(samplerState: samplerState, transformMatrix: baseMatrixWithMainShake);
            {
                foreach (var coin in gc.coins)
                {
                    batch.Draw(coinGraphic, coin.CollisionRect.Location.ToVector2(), Color.White);
                }
            }
            batch.End();

            // Draw paddle background.
            batch.Begin(samplerState: samplerState, transformMatrix: baseScaleMatrix);
            {
                batch.Draw(paddleBackground, new Vector2(0, MonoJam.WINDOW_HEIGHT - MonoJam.PADDLE_AREA_HEIGHT), Color.White);
            }
            batch.End();

            // Draw enemies.
            batch.Begin(samplerState: samplerState, transformMatrix: baseMatrixWithMainShake);
            {
                foreach (var c in gc.corpses)
                {
                    var oldEnemySize = new Vector2(10, 10);
                    var enemyDiff = new Vector2(piggyBankGraphic.Width, piggyBankGraphic.Height) - oldEnemySize;
                    
                    bool isBackwardsVacuum = c.EnemyReference is VacuumEnemy && c.EnemyReference.direction < 0;

                    var fireOffset = new Vector2(-1, -5);
                    if (isBackwardsVacuum)
                    {
                        fireOffset += new Vector2(5, 0);
                    }

                    var effect = c.speed.X < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                    var effectInv = c.speed.X > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                    batch.Draw(enemyFireGraphics[c.animationFrame % enemyFireGraphics.Length],
                        (c.Position + fireOffset).ToPoint().ToVector2(),
                        null,
                        Color.White,
                        0, Vector2.Zero, 1, effect, 0);

                    DrawEnemy(batch, c.EnemyReference, c.Position.ToPoint().ToVector2());
                }
                for (int i = 0; i < gc.totalEnemies; i++)
                {
                    DrawEnemy(batch, gc.enemies[i], gc.enemies[i].CollisionRect.Location.ToVector2());
                }
            }
            batch.End();

            // Draw back of paddle.
            if (gc.currentStage.HasFlag(Stage.StageFlags.PaddlePlayerEnabled))
            {
                batch.Begin(samplerState: samplerState, transformMatrix: baseMatrix);
                {
                    batch.Draw(paddleGraphicBack, gc.paddlePlayer.CollisionRect.Location.ToVector2(), Color.White);
                }
                batch.End();
            }

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
                    if (!note.InsideVacuum)
                    {
                        batch.Draw(noteGraphics[note.Type], note.CollisionRect.Location.ToVector2(), Color.White);
                    }
                }
            }
            batch.End();

            // Draw front of paddle.
            if (gc.currentStage.HasFlag(Stage.StageFlags.PaddlePlayerEnabled))
            {
                batch.Begin(samplerState: samplerState, transformMatrix: baseMatrix);
                {
                    batch.Draw(paddleGraphicFront, gc.paddlePlayer.CollisionRect.Location.ToVector2(), Color.White);
                }
                batch.End();
            }

            var scoreLength = ScoreController.LengthOf(gc.currentCoins / 100d);
            var totalArea = MonoJam.PLAYABLE_AREA_WIDTH - scoreLength;
            var totalAreaRatio = (MonoJam.PLAYABLE_AREA_WIDTH - scoreLength) / (float)MonoJam.PLAYABLE_AREA_WIDTH;

            // Draw hud.
            var laserPercentage = (int)(gc.laserPlayer.laserCharge * totalArea) / (float)totalArea * totalAreaRatio;

            var healthPercentage = (gc.currentStage.MaxNotesMissed - gc.notesMissed) / (float)gc.currentStage.MaxNotesMissed;
            healthPercentage = (int)(healthPercentage * totalArea) / (float)totalArea * totalAreaRatio;

            var totalDuration = gc.currentStage.RequiredTimePassed.TotalMilliseconds;
            var currentDuration = (DateTime.Now - gc.currentStage.startTime).TotalMilliseconds;

            float progressPercentage;

            if (gc.currentStage.HasFlag(Stage.StageFlags.CompleteOnTimePassed))
            {
                progressPercentage = (float)(currentDuration / totalDuration);
            }
            else if (gc.currentStage.HasFlag(Stage.StageFlags.CompleteOnCollectCoins))
            {
                progressPercentage = gc.currentStage.coinsCollected / (float)gc.currentStage.RequiredCoins;
            }
            else
            {
                progressPercentage = 1f;
            }

            progressPercentage = (int)(progressPercentage * totalArea) / (float)totalArea * totalAreaRatio;

            batch.Begin(samplerState: samplerState, transformMatrix: baseScaleMatrix);
            {
                batch.Draw(hudBackground, Vector2.Zero, Color.White);

                batch.Draw(hudLaserCharge,
                    new Vector2(scoreLength, 3),
                    null,
                    Color.White,
                    0, Vector2.Zero,
                    new Vector2(laserPercentage, 1),
                    SpriteEffects.None, 0);

                batch.Draw(hudPlayerHealth,
                    new Vector2(scoreLength, 1),
                    null,
                    Color.White,
                    0, Vector2.Zero,
                    new Vector2(healthPercentage, 1),
                    SpriteEffects.None, 0);

                batch.Draw(hudTimeRemaining,
                    new Vector2(scoreLength, 0),
                    null,
                    Color.White,
                    0, Vector2.Zero,
                    new Vector2(progressPercentage, 1),
                    SpriteEffects.None, 0);

                batch.Draw(scoreBackground, Vector2.Zero, Color.White);
                DrawScore(batch, Vector2.Zero, gc.currentCoins / 100d);
            }
            batch.End();

            // Draw laser player (over HUD).
            if (gc.currentStage.HasFlag(Stage.StageFlags.LaserPlayerEnabled))
            {
                batch.Begin(samplerState: samplerState, transformMatrix: baseMatrixWithLaserShake);
                {
                    batch.Draw(playerGraphic, (gc.laserPlayer.CollisionRect.Location + laserPlayerOffset).ToVector2(), Color.White);
                }
                batch.End();
            }
            
            var mousePos = Mouse.GetState().Position / new Point(MonoJam.SCALE) - new Point(0, MonoJam.PLAYABLE_AREA_Y);

            // TODO: Combine both sets of data, add to texture2D, draw once.
            if (gc.laserPlayer.FiringLaser)
            {
                var laserAlpha = Math.Min(gc.laserPlayer.laserCharge * 8, 1f);
                Color laserFadeColor = new Color(1f, 1f, 1f, laserAlpha);

                var startPos = gc.laserPlayer.LeftEyePos;
                var newData = LineGraphic.CreateLineBoundsCheck(startPos.X, startPos.Y, mousePos.X, mousePos.Y, Color.Red);
                playerLasersLayer.SetData(newData);

                batch.Begin(samplerState: samplerState, transformMatrix: baseMatrixWithLaserShake, blendState: BlendState.NonPremultiplied);
                {
                    batch.Draw(playerLasersLayer, Vector2.Zero, laserFadeColor);
                }
                batch.End();

                startPos = gc.laserPlayer.RightEyePos;
                newData = LineGraphic.CreateLineBoundsCheck(startPos.X, startPos.Y, mousePos.X, mousePos.Y, Color.Red);
                playerLasersLayer.SetData(newData);

                batch.Begin(samplerState: samplerState, transformMatrix: baseMatrixWithLaserShake, blendState: BlendState.NonPremultiplied);
                {
                    batch.Draw(playerLasersLayer, Vector2.Zero, laserFadeColor);
                }
                batch.End();
            }

            batch.Begin(samplerState: samplerState, transformMatrix: baseScaleMatrix);
            {
                batch.Draw(stageOverBackground, new Vector2(0, (int)gc.stageCompleteMenu.currentY) + stageCompleteOffset.ToVector2(), Color.White);
            }
            batch.End();
        }

        public void DrawScore(SpriteBatch b, Vector2 drawPos, double score)
        {
            var scoreStr = ScoreController.StringFor(score);

            for (int i = 0; i < scoreStr.Length; i++)
            {
                // Only attempt to draw character if supported.
                if(fontGraphics.ContainsKey(scoreStr[i]))
                {
                    Vector2 offset = drawPos + new Vector2(ScoreController.OffsetOf(scoreStr, i), 1);
                    b.Draw(fontGraphics[scoreStr[i]], offset, Color.White);
                }
            }
        }

        public void DrawEnemy(SpriteBatch b, Enemy enemy, Vector2 drawPos)
        {
            var effect = enemy.direction < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            var effectInv = enemy.direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            if (enemy is PiggyBank)
            {
                batch.Draw(piggyBankGraphic, drawPos,
                    null,
                    Color.White,
                    0, Vector2.Zero, 1, effectInv, 0);
            }
            else if (enemy is VacuumEnemy)
            {
                var offsetX1 = enemy.direction > 0 ? VacuumEnemy.WIDTH_HEAD : 0;
                var offsetX2 = enemy.direction > 0 ? 0 : VacuumEnemy.WIDTH_HEAD;

                Color[] vacuumNoteData = new Color[VacuumEnemy.MAX_NOTES_HELD * 3];
                for (int j = 0; j < VacuumEnemy.MAX_NOTES_HELD; j++)
                {
                    var colour = noteColours[((VacuumEnemy)enemy).NotesHeld[j]?.Type ?? Note.NoteType.None];

                    vacuumNoteData[j + VacuumEnemy.MAX_NOTES_HELD * 0] = colour;
                    vacuumNoteData[j + VacuumEnemy.MAX_NOTES_HELD * 1] = colour;
                    vacuumNoteData[j + VacuumEnemy.MAX_NOTES_HELD * 2] = colour;
                }

                var thing = new Texture2D(graphicsDevice, VacuumEnemy.MAX_NOTES_HELD, 3);
                thing.SetData(vacuumNoteData);

                Vector2 vacuumNoteGraphicOffset = enemy.direction > 0 ? new Vector2(3, 4) : new Vector2(4, 4);

                b.Draw(thing, drawPos + new Vector2(offsetX2, 0) + vacuumNoteGraphicOffset, Color.White);

                var vacHead = ((VacuumEnemy)enemy).lookingUp ? vacuumHeadUpGraphic : vacuumHeadDownGraphic;

                b.Draw(vacHead, drawPos + new Vector2(offsetX1, 0),
                    null,
                    Color.White,
                    0, Vector2.Zero, 1, effectInv, 0);
                b.Draw(vacuumBodyGraphic, drawPos + new Vector2(offsetX2, 0),
                    null,
                    Color.White,
                    0, Vector2.Zero, 1, effectInv, 0);
            }
        }
    }
}
