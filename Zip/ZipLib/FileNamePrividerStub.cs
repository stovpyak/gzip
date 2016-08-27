namespace ZipLib
{
    public class FileNamePrividerStub: IFileNameProvider
    {
        private readonly string _fileName;

        public FileNamePrividerStub(string fileName)
        {
            _fileName = fileName;
        }

        public string GetFileName()
        {
            return _fileName;
        }
    }
}
