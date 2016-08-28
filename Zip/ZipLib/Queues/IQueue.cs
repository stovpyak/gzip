namespace ZipLib.Queues
{
    public interface IQueue
    {
        string Name { get; }

        FilePart GetPart();

        void Add(FilePart part);
    }
}
