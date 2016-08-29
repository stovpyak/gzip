using System.Threading;

namespace ZipLib.Queues
{
    public interface IQueue
    {
        string Name { get; }

        /// <summary>
        /// Получить элемент из очереди. Может вернуть null
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        FilePart GetPart(object param = null);

        /// <summary>
        /// Добавить один элемент в очередь
        /// </summary>
        /// <param name="part"></param>
        void Add(FilePart part);

        /// <summary>
        /// Кол-во элементов в очереди
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Очередь сообщает всем что ждать её больше не нужно - работа завершена
        /// </summary>
        void NotifyEndWait();

        /// <summary>
        /// Очередь сообщает о своем изменении
        /// </summary>
        AutoResetEvent ChangeEvent { get; }
    }
}
