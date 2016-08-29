using System.Threading;
using ZipLib.Loggers;

namespace ZipLib.Queues
{
    /// <summary>
    /// Базовый класс для очередей
    /// </summary>
    public abstract class PartsBase
    {
        protected readonly ILogger Logger;
        protected readonly object LockOn = new object();

        protected PartsBase(string name, ILogger logger)
        {
            Name = name;
            Logger = logger;
        }

        public string Name { get; }

        public abstract int Count { get; }

        public AutoResetEvent ChangeEvent { get; } = new AutoResetEvent(false);

        /// <summary>
        /// Добавлене элемента в очередь
        /// </summary>
        /// <param name="part"></param>
        public abstract void Add(FilePart part);

        /// <summary>
        /// Сообщить тем, кто ждет появления элементов в очереди, что работа завершена - элементов не будет
        /// </summary>
        public void NotifyEndWait()
        {
            ChangeEvent.Set();
        }
    }
}
