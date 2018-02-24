using Microsoft.Xna.Framework;
using Splerp.Change.Controllers;

namespace Splerp.Change
{
    public sealed class ChangeGame : Game
    {
        public const int SCALE = 8;
        
        public const int WINDOW_WIDTH = 120;
        public const int WINDOW_HEIGHT = 70;

        public const int HUD_HEIGHT = 5;

        public const int PLAYABLE_AREA_WIDTH = 120;
        public const int PLAYABLE_AREA_HEIGHT = WINDOW_HEIGHT - HUD_HEIGHT - PADDLE_AREA_HEIGHT;
        public const int PLAYABLE_AREA_Y = HUD_HEIGHT;
        public const int PADDLE_AREA_HEIGHT = 3;

        GraphicsDeviceManager graphics;

        private GraphicsController grc;
        private InputController ic;
        private GameController gc;
        private SoundController sc;

        public ChangeGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = WINDOW_WIDTH * SCALE;
            graphics.PreferredBackBufferHeight = WINDOW_HEIGHT * SCALE;
            graphics.ApplyChanges();

            var screenWidth = graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width;
            var screenHeight = graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height;

            IsMouseVisible = true;
            Window.Position = new Point(
                (screenWidth - graphics.PreferredBackBufferWidth) / 2,
                (screenHeight - graphics.PreferredBackBufferHeight) / 2 - 100);

            Window.Title = "CHANGE";
        }
        
        protected override void Initialize()
        {
            ic = new InputController();
            grc = new GraphicsController(this, ic, GraphicsDevice);
            sc = new SoundController(this);
            gc = new GameController(this, grc, ic);

            base.Initialize();

            // Set initial state to the main menu.
            gc.SetState(GameState.Title);
        }

        protected override void LoadContent()
        {
            // Load graphics.
            grc.LoadContent(Content);

            // Load audio.
            sc.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            ic.UpdateControlStates();
            gc.Update();

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            grc.Draw();

            base.Draw(gameTime);
        }
    }
}
