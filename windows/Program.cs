using System;
using game;

namespace windows
{
    class Program
    {

        [STAThread]
        static void Main()
        {
            using (var game = new GameApplication())
            {
                game.Run();
            }
        }
    }
}