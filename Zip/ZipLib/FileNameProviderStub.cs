namespace ZipLib
{
    public class FileNameProviderStub: IFileNameProvider
    {
        private readonly string _fileName;

        public FileNameProviderStub(string fileName)
        {
            _fileName = fileName;
        }

        public string GetFileName()
        {
            return _fileName;
        }
    }
}
