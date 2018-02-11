namespace MonoJam
{
    class GameController
    {
        private MonoJam mj;

        public Player player;

        public GameController(MonoJam mjIn)
        {
            mj = mjIn;

            player = new Player();
        }

        public void Update()
        {

        }
    }
}
