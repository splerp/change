using System;

namespace Splerp.Change
{
#if WINDOWS || LINUX
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new ChangeGame())
                game.Run();
        }
    }
#endif
}
