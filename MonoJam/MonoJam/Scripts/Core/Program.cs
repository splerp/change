using System;

namespace MonoJam
{
#if WINDOWS || LINUX
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new MonoJam())
                game.Run();
        }
    }
#endif
}
