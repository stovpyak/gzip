using System;
using System.Threading;

namespace ZipLib
{
    public class AbortExit
    {
        private readonly ManualResetEventSlim _stopEvent;
        public Thread _thread;

        public AbortExit(ManualResetEventSlim stopEvent)
        {
            _stopEvent = stopEvent;
            _thread = new Thread(this.Run) {Name = "AbortExit"};
            _thread.Start();
        }

        protected void Run()
        {
            bool isAbortExit = false;
            while (!isAbortExit)
            {
                var input = Console.ReadKey();
                isAbortExit = (((input.Modifiers & ConsoleModifiers.Control) != 0) &&
                                    (input.Key == ConsoleKey.C));
                Console.WriteLine("нажато не Ctrl+C");
            }
            Console.WriteLine("нажато Ctrl+C");
            _stopEvent.Set();
        }

        public void Stop()
        {
            _thread.Abort();
        }
    }
}
