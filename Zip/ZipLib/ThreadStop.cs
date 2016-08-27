namespace ZipLib
{
    public class ThreadStop
    {
        private readonly object _lockOn = new object();
        private bool _isNeedStop;

        public bool IsNeedStop
        {
            get
            {
                lock (_lockOn)
                {
                    return _isNeedStop;
                }
            }
            set
            {
                lock (_lockOn)
                {
                    _isNeedStop = value;
                }
            }
        }
    }
}
