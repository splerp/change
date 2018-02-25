using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Splerp.Change.Events;
using Splerp.Change.GameObjects;
using Splerp.Change.Graphics;
using Splerp.Change.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splerp.Change.Controllers
{
    public sealed class GraphicsController : IDisposable
    {
        public const int MAX_VAULT_BG_LAYERS_VISIBLE = 10;
        public const int MAX_VAULT_BG_LAYERS = 13;

        public delegate void DrawState();

        private ChangeGame cg;
        private InputController ic;
        private GameController gc;
        
        private GraphicsDevice graphicsDevice;
        private SpriteBatch batch;
        private SamplerState samplerState;

        // The base matrix for game elements. Moves content down
        // to the defined playable area.
        private Matrix baseMatrix;

        // The base matrix for all elements. Scales to required screen scale.
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
        private Texture2D currentCoinBackground;

        private Texture2D changeControlsBackground;
        private Texture2D changeControlsForward;
        private Texture2D changeControlsBack;
        private Texture2D changeControlsLeft;
        private Texture2D changeControlsRight;

        // TODO: Make this a Dictionary which maps to note types.
        private Texture2D[] noteFireGraphics;

        private Texture2D[] enemyFireGraphics;

        private Dictionary<char, Texture2D> fontGraphics;

        private List<CoinBackgroundLayer> coinBackgroundLayers;
        #endregion

        private const int vaultWallWidth = 10;
        private const int vaultFloorHeight = 20;

        // Various offsets for graphics.
        private Point gameOverOffset = new Point(0, -6);
        private Point stageCompleteOffset = new Point(0, -6);
        private Point laserPlayerOffset = new Point(-LaserPlayer.GRAPHIC_OUTER_WIDTH, -LaserPlayer.GRAPHIC_OUTER_WIDTH);
        private Point mutedIconOffset;
        private Point mutedMusicIconOffset;
        private Point noTutorialIconOffset;
        private Point bestScoreBackgroundOffset;

        // Map note types to the textures they use.
        private Dictionary<Note.NoteType, Texture2D> noteGraphics;

        // Map note types to the colour they appear as in vacuums.
        private Dictionary<Note.NoteType, Color> noteColours = new Dictionary<Note.NoteType, Color>
        {
            { Note.NoteType.None, Color.White },
            { Note.NoteType.Pink5, new Color(207, 97, 216) },
            { Note.NoteType.Blue10, new Color(97, 151, 216) },
            { Note.NoteType.Red20,new Color(221, 83, 81) },
            { Note.NoteType.Yellow50, new Color(228, 152, 84) },
            { Note.NoteType.Green100, new Color(81, 231, 97) },
        };

        public GraphicsController(ChangeGame cgIn, InputController icIn, GraphicsDevice graphicsDeviceIn)
        {
            cg = cgIn;
            ic = icIn;

            // Subscribe to coin background events.
            CoinBackgroundController.CoinBufferCompleted += CreateNewCoinBuffer;
            CoinBackgroundController.CurrentCoinBufferUpdated += OnCoinBufferUpdated;

            graphicsDevice = graphicsDeviceIn;
            coinBackgroundLayers = new List<CoinBackgroundLayer>();

            batch = new SpriteBatch(graphicsDevice);
            samplerState = new SamplerState() { Filter = TextureFilter.Point };

            baseScaleMatrix = Matrix.CreateScale(ChangeGame.SCALE);
            baseMatrix =
                  Matrix.CreateTranslation(0, ChangeGame.PLAYABLE_AREA_Y, 0)
                * baseScaleMatrix;
        }

        public void SetGameControllerReference(GameController gcIn)
        {
            gc = gcIn;
        }

        // Remove all background layers. Add the initial end-of-room background.
        public void ResetCoinBuffers()
        {
            coinBackgroundLayers.Clear();
            coinBackgroundLayers.Add(new CoinBackgroundLayer() { graphic = backgroundGraphic });
        }

        // Move the current coin buffer into the background layers collection.
        public void CreateNewCoinBuffer(object sender, EventArgs e)
        {
            Color[] theCurrentData = new Color[ChangeGame.PLAYABLE_AREA_WIDTH * ChangeGame.PLAYABLE_AREA_HEIGHT];

            var NewCoinBackground = new CoinBackgroundLayer
            {
                graphic = new Texture2D(graphicsDevice, ChangeGame.PLAYABLE_AREA_WIDTH, ChangeGame.PLAYABLE_AREA_HEIGHT)
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

        public void OnCoinBufferUpdated(object sender, CoinBufferUpdatedArgs e)
        {
            // Update current coin data.
            currentCoinBackground.SetData(e.updatedBuffer.Select(c => c == 0 ? Color.Transparent : Color.Yellow).ToArray());
        }

        // Load all graphics.
        public void LoadContent(ContentManager Content)
        {
            // Initialise variables but don't set data (need to set data on each draw call).
            playerLasersLayer = new Texture2D(graphicsDevice, ChangeGame.PLAYABLE_AREA_WIDTH, ChangeGame.PLAYABLE_AREA_HEIGHT);
            currentCoinBackground = new Texture2D(graphicsDevice, ChangeGame.PLAYABLE_AREA_WIDTH, ChangeGame.PLAYABLE_AREA_HEIGHT);

            #region Textures from ContentManager
            playerGraphic = Content.Load<Texture2D>("Graphics/Player");
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

            changeControlsBackground = Content.Load<Texture2D>("Graphics/ChangeControlsBackground");
            changeControlsForward = Content.Load<Texture2D>("Graphics/ChangeControlsForward");
            changeControlsBack = Content.Load<Texture2D>("Graphics/ChangeControlsBack");
            changeControlsLeft = Content.Load<Texture2D>("Graphics/ChangeControlsLeft");
            changeControlsRight = Content.Load<Texture2D>("Graphics/ChangeControlsRight");

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

            noteGraphics = new Dictionary<Note.NoteType, Texture2D>
            {
                { Note.NoteType.Pink5, Content.Load<Texture2D>("Graphics/5Dollars") },
                { Note.NoteType.Blue10, Content.Load<Texture2D>("Graphics/10Dollars") },
                { Note.NoteType.Red20, Content.Load<Texture2D>("Graphics/20Dollars") },
                { Note.NoteType.Yellow50, Content.Load<Texture2D>("Graphics/50Dollars") },
                { Note.NoteType.Green100, Content.Load<Texture2D>("Graphics/100Dollars") },
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
            #endregion

            #region Other textures
            coinGraphic = new Texture2D(graphicsDevice, 1, 1);
            coinGraphic.SetData(new Color[] { Color.Yellow });

            VaultWalls = new Texture2D(graphicsDevice, vaultWallWidth, ChangeGame.PLAYABLE_AREA_HEIGHT);
            VaultWalls.SetData(Enumerable.Repeat(new Color(59, 33, 12), VaultWalls.Width * VaultWalls.Height).ToArray());

            VaultFloor = new Texture2D(graphicsDevice, ChangeGame.PLAYABLE_AREA_WIDTH + vaultWallWidth * 2, vaultFloorHeight);
            VaultFloor.SetData(Enumerable.Repeat(new Color(36, 17, 1), VaultFloor.Width * VaultFloor.Height).ToArray());

            hudBackground = new Texture2D(graphicsDevice, ChangeGame.PLAYABLE_AREA_WIDTH, ChangeGame.HUD_HEIGHT);
            hudBackground.SetData(Enumerable.Repeat(Color.Black, hudBackground.Width * hudBackground.Height).ToArray());

            hudLaserCharge = new Texture2D(graphicsDevice, ChangeGame.PLAYABLE_AREA_WIDTH, 2);
            hudLaserCharge.SetData(Enumerable.Repeat(Color.Red, hudLaserCharge.Width * hudLaserCharge.Height).ToArray());

            hudPlayerHealth = new Texture2D(graphicsDevice, ChangeGame.PLAYABLE_AREA_WIDTH, 2);
            var hpBar = Enumerable.Repeat(Color.Green, hudPlayerHealth.Width * (hudPlayerHealth.Height - 1))
                .Concat(Enumerable.Repeat(Color.DarkGreen, hudPlayerHealth.Width * 1))
                .ToArray();
            hudPlayerHealth.SetData(hpBar);

            hudTimeRemaining = new Texture2D(graphicsDevice, ChangeGame.PLAYABLE_AREA_WIDTH, 1);
            hudTimeRemaining.SetData(Enumerable.Repeat(Color.Blue, hudTimeRemaining.Width * hudTimeRemaining.Height).ToArray());
            #endregion

            // Set offsets.
            bestScoreBackgroundOffset = new Point(ChangeGame.WINDOW_WIDTH - 2, 2);
            mutedIconOffset = new Point(ChangeGame.WINDOW_WIDTH - mutedIcon.Width - 3, ChangeGame.WINDOW_HEIGHT - mutedIcon.Height - 3);
            mutedMusicIconOffset = new Point(ChangeGame.WINDOW_WIDTH - mutedIcon.Width - 3, ChangeGame.WINDOW_HEIGHT - mutedIcon.Height * 2 - 3 - 2);
            noTutorialIconOffset = new Point(ChangeGame.WINDOW_WIDTH - noTutorialIcon.Width * 2 - 3 - 2, ChangeGame.WINDOW_HEIGHT - noTutorialIcon.Height - 3);

            // Set draw functions on game states.
            GameState.Title.Draw = DrawMainMenu;
            GameState.MapControls.Draw = DrawChangeControlsMenu;
            GameState.Playing.Draw = DrawGame;
            GameState.BetweenStages.Draw = DrawGame;
            GameState.GameOver.Draw = DrawGameOverMenu;
        }

        public void Draw()
        {
            gc.CurrentState.Draw();
        }

        #region State "Draw" methods.
        public void DrawMainMenu()
        {
            // Set "cursor" position on menu.
            Vector2 coinPos = Vector2.Zero;
            switch (gc.CurrentMenu.SelectedOption)
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

                var totalScoreLength = 11 + ScoreRenderer.LengthOf(gc.bestCoinScore / 100d);
                var diff = bestScoreBackgroundOffset.X - totalScoreLength;

                // Draw "best score" section.
                if(gc.bestCoinScore > 0)
                {
                    batch.Draw(bestScoreBackground, bestScoreBackgroundOffset.ToVector2() - new Vector2(totalScoreLength, 0), Color.White);
                    DrawScore(batch, bestScoreBackgroundOffset.ToVector2() + new Vector2(11, 2) - new Vector2(totalScoreLength, 0), gc.bestCoinScore / 100d);
                }

                #region Main menu icons
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
                #endregion

                // Draw the border graphic over the bestScoreBackground.
                batch.Draw(titleBorderGraphic, Vector2.Zero, Color.White);
            }
            batch.End();
        }

        public void DrawChangeControlsMenu()
        {
            if(!ic.FinishedRemapping)
            {
                // Choose which controlDirective to display.
                var currentControl = ic.CurrentRemappingControl;
                Texture2D controlDirective = null;
                if (currentControl == Control.MoveUp)
                {
                    controlDirective = changeControlsForward;
                }
                else if (currentControl == Control.MoveDown)
                {
                    controlDirective = changeControlsBack;
                }
                else if (currentControl == Control.MoveLeft)
                {
                    controlDirective = changeControlsLeft;
                }
                else if (currentControl == Control.MoveRight)
                {
                    controlDirective = changeControlsRight;
                }

                batch.Begin(samplerState: samplerState, transformMatrix: baseScaleMatrix);
                {
                    batch.Draw(changeControlsBackground, Vector2.Zero, Color.White);

                    if (controlDirective != null)
                    {
                        batch.Draw(controlDirective, Vector2.Zero, Color.White);
                    }
                }
                batch.End();
            }
        }

        public void DrawGameOverMenu()
        {
            DrawGame();

            // Set "cursor" position on menu.
            Vector2 coinPos = Vector2.Zero;
            switch (gc.CurrentMenu.SelectedOption)
            {
                case 0:
                    coinPos = new Vector2(33, 30);
                    break;
                case 1:
                    coinPos = new Vector2(34, 46);
                    break;
            }

            var menuPos = (gameOverOffset + gc.CurrentMenu.MenuOffset.ToPoint()).ToVector2();

            batch.Begin(samplerState: samplerState, transformMatrix: baseScaleMatrix);
            {
                batch.Draw(gameOverBackground, menuPos, Color.White);
                batch.Draw(titleCursor, coinPos + menuPos, Color.White);
            }
            batch.End();
        }

        public void DrawGame()
        {
            var baseMatrixWithLaserShake = Matrix.CreateTranslation(
                new Vector3(gc.laserPlayer.laserShake.CurrentShake, 0))
                * baseMatrix;

            var mousePos = InputController.CurrentMousePosition;

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
                      Matrix.CreateTranslation(-ChangeGame.WINDOW_WIDTH / 2, -ChangeGame.PLAYABLE_AREA_HEIGHT, 0)
                    * baseMatrix
                    * Matrix.CreateScale(coinBackground.currentScale)
                    * Matrix.CreateTranslation(
                        ChangeGame.PLAYABLE_AREA_WIDTH * ChangeGame.SCALE / 2,
                        ChangeGame.PLAYABLE_AREA_HEIGHT * ChangeGame.SCALE - coinBackground.currentTranslate,
                        0);

                // Draw buffer coin background.
                batch.Begin(
                    sortMode: SpriteSortMode.BackToFront,
                    samplerState: samplerState,
                    transformMatrix: coinBackgroundMatrix);
                {
                    batch.Draw(coinBackground.graphic, new Vector2(0, 0), drawColour);
                    batch.Draw(VaultWalls, new Vector2(-vaultWallWidth, 0), drawColour);
                    batch.Draw(VaultWalls, new Vector2(ChangeGame.PLAYABLE_AREA_WIDTH, 0), drawColour);
                    batch.Draw(VaultFloor, new Vector2(-vaultWallWidth, -vaultFloorHeight), drawColour);
                    batch.Draw(VaultFloor, new Vector2(-vaultWallWidth, ChangeGame.PLAYABLE_AREA_HEIGHT), drawColour);
                }
                batch.End();
            }
            
            // Draw current coin background.
            batch.Begin(samplerState: samplerState, transformMatrix: baseMatrix);
            {
                batch.Draw(currentCoinBackground, new Vector2(0, 0), Color.White);
            }
            batch.End();

            // Draw falling coins.
            batch.Begin(samplerState: samplerState, transformMatrix: baseMatrix);
            {
                foreach (var coin in gc.FallingCoins)
                {
                    batch.Draw(coinGraphic, coin.CollisionRect.Location.ToVector2(), Color.White);
                }
            }
            batch.End();

            // Draw paddle background.
            batch.Begin(samplerState: samplerState, transformMatrix: baseScaleMatrix);
            {
                batch.Draw(paddleBackground, new Vector2(0, ChangeGame.WINDOW_HEIGHT - ChangeGame.PADDLE_AREA_HEIGHT), Color.White);
            }
            batch.End();

            // Draw enemies.
            batch.Begin(samplerState: samplerState, transformMatrix: baseMatrix);
            {
                foreach (var c in gc.corpses)
                {
                    var oldEnemySize = new Vector2(10, 10);
                    var enemyDiff = new Vector2(piggyBankGraphic.Width, piggyBankGraphic.Height) - oldEnemySize;
                    
                    bool isBackwardsVacuum = c.EnemyReference is VacuumEnemy && c.EnemyReference.Direction < 0;

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
                for (int i = 0; i < gc.enemies.Count; i++)
                {
                    DrawEnemy(batch, gc.enemies[i], gc.enemies[i].CollisionRect.Location.ToVector2());
                }
            }
            batch.End();

            // Draw back of paddle.
            if (gc.CurrentStage.HasFlag(Stage.StageFlags.PaddlePlayerEnabled))
            {
                batch.Begin(samplerState: samplerState, transformMatrix: baseMatrix);
                {
                    batch.Draw(paddleGraphicBack, gc.paddlePlayer.CollisionRect.Location.ToVector2(), Color.White);
                }
                batch.End();
            }

            // Draw money.
            batch.Begin(samplerState: samplerState, transformMatrix: baseMatrix);
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
            if (gc.CurrentStage.HasFlag(Stage.StageFlags.PaddlePlayerEnabled))
            {
                batch.Begin(samplerState: samplerState, transformMatrix: baseMatrix);
                {
                    batch.Draw(paddleGraphicFront, gc.paddlePlayer.CollisionRect.Location.ToVector2(), Color.White);
                }
                batch.End();
            }

            var scoreLength = ScoreRenderer.LengthOf(gc.currentCoinScore / 100d);
            var totalArea = ChangeGame.PLAYABLE_AREA_WIDTH - scoreLength;
            var totalAreaRatio = (ChangeGame.PLAYABLE_AREA_WIDTH - scoreLength) / (float)ChangeGame.PLAYABLE_AREA_WIDTH;

            #region Draw hud
            // Calculate values for drawing the hud.
            var laserPercentage = (int)(gc.laserPlayer.laserCharge * totalArea) / (float)totalArea * totalAreaRatio;

            var healthPercentage = (gc.CurrentStage.MaxNotesMissed - gc.notesMissed) / (float)gc.CurrentStage.MaxNotesMissed;
            healthPercentage = (int)(healthPercentage * totalArea) / (float)totalArea * totalAreaRatio;

            var totalDuration = gc.CurrentStage.RequiredTimePassed.TotalMilliseconds;
            var currentDuration = (DateTime.Now - gc.CurrentStage.startTime).TotalMilliseconds;

            float progressPercentage;

            if (gc.CurrentStage.HasFlag(Stage.StageFlags.CompleteOnTimePassed))
            {
                progressPercentage = (float)(currentDuration / totalDuration);
            }
            else if (gc.CurrentStage.HasFlag(Stage.StageFlags.CompleteOnCollectCoins))
            {
                progressPercentage = gc.CurrentStage.coinsCollected / (float)gc.CurrentStage.RequiredCoins;
            }
            else
            {
                progressPercentage = 1f;
            }

            progressPercentage = (int)(progressPercentage * totalArea) / (float)totalArea * totalAreaRatio;

            // Draw the hud.
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
                DrawScore(batch, Vector2.Zero, gc.currentCoinScore / 100d);
            }
            batch.End();
            #endregion

            // Draw laser player (over HUD).
            if (gc.CurrentStage.HasFlag(Stage.StageFlags.LaserPlayerEnabled))
            {
                batch.Begin(samplerState: samplerState, transformMatrix: baseMatrixWithLaserShake);
                {
                    batch.Draw(playerGraphic, (gc.laserPlayer.CollisionRect.Location + laserPlayerOffset).ToVector2(), Color.White);
                }
                batch.End();
            }
            
            // Draw lasers.
            if (gc.laserPlayer.FiringLaser)
            {
                Color[] laserData = Enumerable.Repeat(Color.Transparent,
                    ChangeGame.PLAYABLE_AREA_WIDTH * ChangeGame.PLAYABLE_AREA_HEIGHT).ToArray();
                
                // Add both lasers to colour data.
                LineGraphic.CreateLineBoundsCheck(laserData,
                    gc.laserPlayer.LeftEyePos.X, gc.laserPlayer.LeftEyePos.Y,
                    mousePos.X, mousePos.Y, Color.Red);
                LineGraphic.CreateLineBoundsCheck(laserData,
                    gc.laserPlayer.RightEyePos.X, gc.laserPlayer.RightEyePos.Y,
                    mousePos.X, mousePos.Y, Color.Red);

                playerLasersLayer.SetData(laserData);

                batch.Begin(samplerState: samplerState, transformMatrix: baseMatrixWithLaserShake, blendState: BlendState.NonPremultiplied);
                {
                    var laserAlpha = Math.Min(gc.laserPlayer.laserCharge * 8, 1f);
                    Color laserFadeColor = new Color(1f, 1f, 1f, laserAlpha);

                    batch.Draw(playerLasersLayer, Vector2.Zero, laserFadeColor);
                }
                batch.End();
            }

            // If there is a Stage Complete menu, draw it.
            if(gc.CurrentMenu != null && gc.CurrentMenu is StageCompleteMenu)
            {
                batch.Begin(samplerState: samplerState, transformMatrix: baseScaleMatrix);
                {
                    batch.Draw(stageOverBackground, gc.CurrentMenu.MenuOffset.ToPoint().ToVector2() + stageCompleteOffset.ToVector2(), Color.White);
                }
                batch.End();
            }
        }
        #endregion

        #region Other draw methods
        public void DrawScore(SpriteBatch b, Vector2 drawPos, double score)
        {
            var scoreStr = ScoreRenderer.StringFor(score);

            for (int i = 0; i < scoreStr.Length; i++)
            {
                // Only attempt to draw character if supported.
                if(fontGraphics.ContainsKey(scoreStr[i]))
                {
                    Vector2 offset = drawPos + new Vector2(ScoreRenderer.OffsetOf(scoreStr, i), 1);
                    b.Draw(fontGraphics[scoreStr[i]], offset, Color.White);
                }
            }
        }

        public void DrawEnemy(SpriteBatch b, Enemy enemy, Vector2 drawPos)
        {
            var effect = enemy.Direction < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            var effectInv = enemy.Direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            if (enemy is PiggyBank)
            {
                batch.Draw(piggyBankGraphic, drawPos,
                    null,
                    Color.White,
                    0, Vector2.Zero, 1, effectInv, 0);
            }
            else if (enemy is VacuumEnemy)
            {
                var offsetX1 = enemy.Direction > 0 ? VacuumEnemy.WIDTH_HEAD : 0;
                var offsetX2 = enemy.Direction > 0 ? 0 : VacuumEnemy.WIDTH_HEAD;

                Color[] vacuumNoteData = new Color[VacuumEnemy.MAX_NOTES_HELD * 3];
                for (int j = 0; j < VacuumEnemy.MAX_NOTES_HELD; j++)
                {
                    var colour = noteColours[((VacuumEnemy)enemy).NotesHeld[j]?.Type ?? Note.NoteType.None];

                    vacuumNoteData[j + VacuumEnemy.MAX_NOTES_HELD * 0] = colour;
                    vacuumNoteData[j + VacuumEnemy.MAX_NOTES_HELD * 1] = colour;
                    vacuumNoteData[j + VacuumEnemy.MAX_NOTES_HELD * 2] = colour;
                }

                var vacuumNotesDisplay = new Texture2D(graphicsDevice, VacuumEnemy.MAX_NOTES_HELD, 3);
                vacuumNotesDisplay.SetData(vacuumNoteData);

                Vector2 vacuumNoteGraphicOffset = enemy.Direction > 0 ? new Vector2(3, 4) : new Vector2(4, 4);

                b.Draw(vacuumNotesDisplay, drawPos + new Vector2(offsetX2, 0) + vacuumNoteGraphicOffset, Color.White);
                
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
        #endregion
        
        #region Disposal handling
        ~GraphicsController()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                batch.Dispose();

                // Dispose of all Texture2D references that were constructed here.
                coinGraphic.Dispose();
                playerLasersLayer.Dispose();
                currentCoinBackground.Dispose();
                VaultWalls.Dispose();
                VaultFloor.Dispose();
                hudBackground.Dispose();
                hudLaserCharge.Dispose();
                hudPlayerHealth.Dispose();
                hudTimeRemaining.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
