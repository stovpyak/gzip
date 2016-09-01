namespace ZipLib
{
    /// <summary>
    /// Часть файла - минимальная единица обрабатываемых данных
    /// </summary>
    public class FilePart
    {
        public FilePart(string name)
        {
            Name = name;
            Index = -1;
        }

        /// <summary>
        /// Уникальное имя части. По нему отследиваем "жизненный цикл" частей
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Индекс фасти файла. порядковый номер части при чтении из файла
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Признак того, что часть последняя
        /// </summary>
        public bool IsLast { get; set; }

        /// <summary>
        /// Размер части файла
        /// </summary>
        public int SourceSize { get; set; }

        /// <summary>
        /// То что прочитали из файла
        /// </summary>
        public byte[] Source { get; set; }

        /// <summary>
        /// То что получилось в результате обработки (архивирования)
        /// </summary>
        public byte[] Result { get; set; }

        public override string ToString()
        {
            return $"{Name}; index={Index};";
        }
    }
}
