namespace ZipLib.Loggers
{
    public class LoggerDummy: ILogger
    {
        public void Add(string msg)
        {
            // empty, так как dummy
        }

        public void Close()
        {
            // empty, так как dummy
        }
    }
}
