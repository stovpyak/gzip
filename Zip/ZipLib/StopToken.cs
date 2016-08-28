using System.Threading;

namespace ZipLib
{
    public class StopToken
    {
        private readonly object _lockOn = new object();

        public void GetEnd()
        {
            lock(_lockOn)
            {
                Monitor.Pulse(_lockOn);
                Monitor.Wait(_lockOn);
            }
        }

        public void OnEventEnd()
        {
            lock (_lockOn)
            {
                Monitor.Pulse(_lockOn);
            }
        }


    }
}
