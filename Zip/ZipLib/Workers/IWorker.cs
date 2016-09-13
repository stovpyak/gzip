namespace ZipLib.Workers
{
    public interface IWorker
    {
        void ProcessPart(FilePart part);

        string Name { get; }
    }
}
