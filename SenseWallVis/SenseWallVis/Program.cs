using System;

namespace SenseWallVis
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (SenseWallVisApp game = new SenseWallVisApp())
            {
                game.Run();
            }
        }
    }
#endif
}

