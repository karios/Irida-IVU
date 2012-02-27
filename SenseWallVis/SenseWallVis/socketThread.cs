using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseWallVis
{
        public class socketThread
        {
            // This method will be called when the thread is started.
            public void DoWork()
            {
                while (!_shouldStop)
                {
                    Console.WriteLine("worker thread: working...");
                }
            }
            public void RequestStop()
            {
                _shouldStop = true;
            }
            // Volatile is used as hint to the compiler that this data
            // member will be accessed by multiple threads.
            private volatile bool _shouldStop;
        }
    
}
