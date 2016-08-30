namespace ZipLib.Decompress
{
    /// <summary>
    /// Результат поиска заголовка
    /// </summary>
    public class TitleSearchResult
    {
        public static TitleSearchResult MakeByIndexTitle(int value)
        {
            var instance = new TitleSearchResult
            {
                IndexStartTitle = value,
                Mode = TitleMode.AllTitle
            };
            return instance;
        }

        public static TitleSearchResult MakeByIndexPartTitle(int value)
        {
            var instance = new TitleSearchResult
            {
                IndexStartTitle = value,
                Mode = TitleMode.PartTitle
            };
            return instance;
        }

        public static TitleSearchResult MakeNotFount()
        {
            return new TitleSearchResult();
        }

        private TitleSearchResult()
        {
            IndexStartTitle = -1;
        }

        public TitleMode Mode { get; set; }

        public int IndexStartTitle { get; set; }
        
        public bool IsNotFound()
        {
            return (IndexStartTitle == -1);
        }
    }

    public enum TitleMode
    {
        AllTitle,
        PartTitle,
    };
}
