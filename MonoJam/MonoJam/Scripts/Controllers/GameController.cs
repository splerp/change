using System;

namespace MonoJam
{
    class GameController
    {
        private MonoJam mj;

        public Player player;
        public Enemy[] enemies;

        public GameController(MonoJam mjIn)
        {
            mj = mjIn;

            player = new Player();
        }

        public void Update()
        {
            // Always keep console window clear.
            Console.Clear();

            player.Update();
        }
    }
}
