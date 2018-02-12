using Microsoft.Xna.Framework;

namespace MonoJam
{
    public class ShakeController
    {
        public float currentAmplitude;
        public float pos;

        public Vector2 CurrentShake => new Vector2(currentAmplitude * Perlin.Noise(pos, 0), currentAmplitude * Perlin.Noise(0, pos));
        
        public ShakeController()
        {
            currentAmplitude = 25f;
        }

        public void Update()
        {
            currentAmplitude--;
            if(currentAmplitude < 0)
            {
                currentAmplitude = 0;
            }

            pos += 0.8f;
        }
    }
}
