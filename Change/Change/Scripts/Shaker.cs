using Microsoft.Xna.Framework;
using Splerp.Change.Utils;

namespace Splerp.Change
{
    public sealed class Shaker
    {
        public float currentAmplitude;
        public float pos;

        public Vector2 CurrentShake => new Vector2(currentAmplitude * Perlin.Noise(pos, 0), currentAmplitude * Perlin.Noise(0, pos));

        public void Update()
        {
            currentAmplitude-= 0.2f;
            if(currentAmplitude < 0)
            {
                currentAmplitude = 0;
            }

            pos += 0.8f;
        }
    }
}
