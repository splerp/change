using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

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
        private Texture2D coinGraphic;
        private Texture2D coinBGGraphic;
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

            coinGraphic = new Texture2D(graphicsDevice, Coin.COIN_WIDTH, 1);
            coinGraphic.SetData(Enumerable.Repeat(Color.Yellow, Coin.COIN_WIDTH).ToArray());

            coinBGGraphic = new Texture2D(graphicsDevice, MonoJam.WINDOW_WIDTH, MonoJam.WINDOW_HEIGHT);
        }

        public void Draw()
        {
            graphicsDevice.Clear(Color.Black);

            // Update data.
            var data = gc.coinData.Select(c => c == 0 ? Color.Black : Color.Yellow).ToArray();
            coinBGGraphic.SetData(data);

            batch.Begin(samplerState: samplerState, transformMatrix: scaler);
            {
                batch.Draw(coinBGGraphic, new Vector2(0, 0), Color.White);
            }
            batch.End();

            batch.Begin(samplerState: samplerState, transformMatrix: scaler);
            {
                batch.Draw(playerGraphic, gc.player.CollisionRect.Location.ToVector2(), Color.White);
            }

            foreach(var coin in gc.coins)
            {
                batch.Draw(coinGraphic, coin.CollisionRect.Location.ToVector2(), Color.White);
            }

            batch.End();
        }
    }
}
