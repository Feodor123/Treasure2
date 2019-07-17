using System;

namespace TreasurePC
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            Console.WriteLine("wwhait!");
            using (var game = new Game1())
                game.Run();
        }
    }
}
