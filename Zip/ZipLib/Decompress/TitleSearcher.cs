using System.Collections.Generic;

namespace ZipLib.Decompress
{
    /// <summary>
    /// Ищем заголовки в прочитанной порции
    /// </summary>
    public class TitleSearcher
    {
        public static readonly int[] Title;

        static TitleSearcher()
        {
            Title = new[] { 31, 139, 8, 0, 0, 0, 0, 0, 4, 0 };
        }

        /// <summary>
        /// Ищет заголовки в прочитанной части
        /// </summary>
        /// <param name="portion"></param>
        /// <returns>Возвращает null, если заголовков совсем нет. 
        /// Если есть, то возвращает список результатов поиска, где один элемент - это инфо по одному заголовку</returns>
        public static List<TitleSearchResult> GetIndexTitle(byte[] portion)
        {
            List<TitleSearchResult> result = null;

            var count = 0;
            for (var i = 0; i < portion.Length; i++)
            {
                if (portion[i] == Title[count])
                {
                    count++;
                    if (count == 10)
                    {
                        // нашли title полностью - добавляем его в результат
                        if (result == null)
                            result = new List<TitleSearchResult>();
                        result.Add(TitleSearchResult.MakeByIndexTitle(i - Title.Length + 1));
                        count = 0;
                    }
                }
                else
                {
                    count = 0;
                    if (portion[i] == Title[count])
                        count++;
                }
            }

            if (count == 0)
                return result;

            // нашли часть заголовка в конце
            if (result == null)
                result = new List<TitleSearchResult>();
            result.Add(TitleSearchResult.MakeByIndexPartTitle(portion.Length - count));
            return result;
        }
    }
}
