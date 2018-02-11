using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MonoJam
{
    class GraphicsController
    {
        private GameController gc;
        private GraphicsDevice graphicsDevice;
        private SpriteBatch batch;
        private SamplerState samplerState;
        private Matrix scaler;

        #region Game graphics
        private Texture2D playerGraphic;
        #endregion

        public GraphicsController(GameController gcIn, GraphicsDevice graphicsDeviceIn)
        {
            gc = gcIn;
            graphicsDevice = graphicsDeviceIn;

            batch = new SpriteBatch(graphicsDevice);
            samplerState = new SamplerState() { Filter = TextureFilter.Point };
            scaler = Matrix.CreateScale(MonoJam.SCALE);
        }

        public void LoadContent(ContentManager Content)
        {
            playerGraphic = Content.Load<Texture2D>("Graphics/Player");
        }

        public void Draw()
        {
            graphicsDevice.Clear(Color.Black);

            batch.Begin(samplerState: samplerState, transformMatrix: scaler);
            {
                batch.Draw(playerGraphic, gc.player.CollisionRect.Location.ToVector2(), Color.White);
            }
            batch.End();
        }
    }
}
