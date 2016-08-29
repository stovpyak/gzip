namespace ZipLib.QueueHandlers
{
    public interface IQueueHandler
    {
        void SetIsNeedStop();
        void Join();
    }
}
