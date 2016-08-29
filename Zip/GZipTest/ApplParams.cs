using ZipLib;

namespace GZipTest
{
    /// <summary>
    /// Параметры для программы
    /// </summary>
    public class ApplParams
    {
        public ApplMode ApplMode { get; set; }
        public string SourceFileName { get; set; }
        public string TargetFileName { get; set; }
    }
}
