using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoJam
{
    class GraphicsController
    {
        private GameController gc;
        private GraphicsDevice graphicsDevice;

        public GraphicsController(GameController gcIn, GraphicsDevice graphicsDeviceIn)
        {
            gc = gcIn;
            graphicsDevice = graphicsDeviceIn;
        }

        public void Draw()
        {
            graphicsDevice.Clear(Color.CornflowerBlue);
        }
    }
}
