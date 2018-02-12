using Microsoft.Xna.Framework;

namespace MonoJam
{
    public class MonoJam : Game
    {
        public const int SCALE = 8;
        
        public const int PLAYABLE_AREA_WIDTH = 120;
        public const int PLAYABLE_AREA_HEIGHT = 70;
        public const int PLAYABLE_AREA_Y = 0;
        public const int WINDOW_WIDTH = 120;
        public const int WINDOW_HEIGHT = PLAYABLE_AREA_Y + PLAYABLE_AREA_HEIGHT;

        GraphicsDeviceManager graphics;

        public GraphicsController grc;
        GameController gc;

        public MonoJam()
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
        }
        
        protected override void Initialize()
        {
            gc = new GameController(this);
            grc = new GraphicsController(gc, GraphicsDevice);

            grc.LoadContent(Content);

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
