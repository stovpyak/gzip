﻿namespace ZipLib.Decompress
{
    /// <summary>
    /// Результат поиска заголовка
    /// </summary>
    public class TitleInfo
    {
        public static TitleInfo MakeByIndexTitle(int value)
        {
            var instance = new TitleInfo
            {
                IndexStartTitle = value,
                Mode = TitleMode.AllTitle
            };
            return instance;
        }

        public static TitleInfo MakeByIndexPartTitle(int value)
        {
            var instance = new TitleInfo
            {
                IndexStartTitle = value,
                Mode = TitleMode.PartTitle
            };
            return instance;
        }

        private TitleInfo()
        {
            IndexStartTitle = -1;
        }

        public TitleMode Mode { get; set; }

        public int IndexStartTitle { get; set; }
    }

    public enum TitleMode
    {
        AllTitle,
        PartTitle,
    };
}
