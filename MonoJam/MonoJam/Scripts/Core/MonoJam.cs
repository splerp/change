using Microsoft.Xna.Framework;

namespace MonoJam
{
    public class MonoJam : Game
    {
        public const int WINDOW_WIDTH = 500;
        public const int WINDOW_HEIGHT = 300;

        GraphicsDeviceManager graphics;

        GraphicsController grc;
        GameController gc;

        public MonoJam()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
            graphics.ApplyChanges();

            var screenWidth = graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width;
            var screenHeight = graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height;

            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.Position = new Point((screenWidth - WINDOW_WIDTH) / 2, (screenHeight - WINDOW_HEIGHT) / 2 - 100);
        }
        
        protected override void Initialize()
        {
            gc = new GameController(this);
            grc = new GraphicsController(gc, GraphicsDevice);

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
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
