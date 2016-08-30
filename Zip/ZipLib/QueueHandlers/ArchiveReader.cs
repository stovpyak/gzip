using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ZipLib.Decompress;
using ZipLib.Loggers;
using ZipLib.Queues;

namespace ZipLib.QueueHandlers
{
    /// <summary>
    /// Читает части архива
    /// </summary>
    public class ArchiveReader: QueueHandlerBase
    {
        private readonly IFileNameProvider _archiveFileNameProvider;
        private FileStream _archiveStream;

        public ArchiveReader(ILogger logger, IFileNameProvider archiveFileNameProvider, IQueue sourceQueue, IQueue nextQueue) : base(logger, sourceQueue, nextQueue)
        {
            _archiveFileNameProvider = archiveFileNameProvider;

            InnerThread = new Thread(this.Run) { Name = "ArchiveReader" };
            InnerThread.Start();
        }

        readonly Stopwatch _processingStopwatch = new Stopwatch();

        private const int SizeOnePortion = 1000;
        private MemoryStream _dataFromPreviousPortion; //todo пока в виде потока, но есть перобразования byte[] <-> stream
        private List<TitleSearchResult> _searchResultFromPreviousPortion;

        protected override bool ProcessPart(FilePart part)
        {
            // !!! код получается ахтунг - начал прятать логиую в абстракцию ArhivePortion

            Logger.Add($"Поток {Thread.CurrentThread.Name} начал читать");
            if (_archiveStream == null)
                _archiveStream = new FileStream(_archiveFileNameProvider.GetFileName(), FileMode.Open, FileAccess.Read);
            try
            {
                //_processingStopwatch.Reset();
                //_processingStopwatch.Start();

                // одна часть архива - она полностью пойдет на декомпрессию
                var currentArchivePart = new MemoryStream();
                var currentArchivePartIsEmpty = true;

                byte[] buffer = new byte[SizeOnePortion];
                while (currentArchivePart != null)
                {
                    // читаем порцию из файла
                    var nRead = _archiveStream.Read(buffer, 0, buffer.Length);
                    if (nRead > 0)
                    {
                        var arhivePortion = new ArhivePortion(buffer, nRead);


                        // теперь нужно посмотреть есть ли в этой порции 10 заветных байт
                        var searchResult = TitleSearcher.GetIndexTitle(buffer);

                        // не нашли заголовков
                        if (searchResult == null)
                        {
                            if (currentArchivePartIsEmpty)
                                throw new Exception("в порции нет заголовка, а часть ещё пустая");
                            // всю прочитанную порцию архива в часть
                            currentArchivePart.Write(buffer, 0, nRead);
                            currentArchivePartIsEmpty = false;
                        }
                        else
                        {
                            var titleInfo = searchResult[0];
                            
                            // нашли заголовок полностью
                            if (titleInfo.Mode == TitleMode.AllTitle)
                            {
                                // заголовок в начале
                                if (titleInfo.IndexStartTitle == 0)
                                {
                                    // нашли заголовок для текущей части
                                    if (currentArchivePartIsEmpty)
                                    {
                                        // больше в порции заголовков нет - записываем заголовок и всё что до конца порции
                                        if (searchResult.Count == 0)
                                        {
                                            currentArchivePart.Write(buffer, 0, buffer.Length);
                                            currentArchivePartIsEmpty = false;
                                        }
                                        else
                                        {
                                            // в порции ещё есть заголовки - записываем в часть все что до следеющего заголовка
                                            var titleInfoForNextPart = searchResult[1];
                                            currentArchivePart.Write(buffer, 0, titleInfoForNextPart.IndexStartTitle);
                                            currentArchivePartIsEmpty = false;

                                            // а остаток нужно "припасти" для следующей part
                                            _dataFromPreviousPortion = new MemoryStream();
                                            _dataFromPreviousPortion.Write(buffer, titleInfoForNextPart.IndexStartTitle,
                                                buffer.Length - titleInfoForNextPart.IndexStartTitle + 1);
                                            // и результат поиска тоже нужно запасти для следующеё части - зачем ещё раз искать заголовки
                                            searchResult.Remove(titleInfo);
                                            _searchResultFromPreviousPortion = searchResult;

                                            part.Source = currentArchivePart.ToArray();
                                            currentArchivePart.Close();
                                            currentArchivePart = null;
                                            currentArchivePartIsEmpty = true;
                                        }
                                    }
                                    // нашли заголовок для следующей части - значит текущая часть сформирована
                                    else
                                    {
                                        // а порцию нужно "припасти" для следующей part
                                        _dataFromPreviousPortion = new MemoryStream();
                                        _dataFromPreviousPortion.Write(buffer, 0, buffer.Length);
                                        // и результат поиска тоже нужно запасти для следующеё части - зачем ещё раз искать заголовки
                                        _searchResultFromPreviousPortion = searchResult;

                                        part.Source = currentArchivePart.ToArray();
                                        currentArchivePart.Close();
                                        currentArchivePart = null;
                                        currentArchivePartIsEmpty = true;
                                    }
                                }
                                else
                                {
                                    // заголовок не в начале порции
                                    if (currentArchivePartIsEmpty)
                                        throw new Exception("В порции заголовок не в начале, а часть ещё пустая");
                                    // то что до заголовка в текущую часть
                                    currentArchivePart.Write(buffer, 0, titleInfo.IndexStartTitle);
                                    currentArchivePartIsEmpty = false;

                                    // "припасти" для следующей part - заголовок и всё что до конца порции
                                    _dataFromPreviousPortion = new MemoryStream();
                                    _dataFromPreviousPortion.Write(buffer, titleInfo.IndexStartTitle,
                                        buffer.Length - titleInfo.IndexStartTitle + 1);
                                    // и результат поиска тоже нужно запасти для следующеё части - зачем ещё раз искать заголовки
                                    searchResult.Remove(titleInfo);
                                    _searchResultFromPreviousPortion = searchResult;
                                }
                            }
                            else
                            {
                                // нашли заголовок не полностью
                                // !!! окончить текущую часть не имеем права - не удостоверившись, что это заголовок

                            }
                        }
                    }
                    else
                    {
                        // архив закончился
                        part.Source = currentArchivePart.ToArray();
                        currentArchivePart.Close();
                        currentArchivePart = null;
                        currentArchivePartIsEmpty = true;
                    }
                }


                //_processingStopwatch.Stop();
                //Logger.Add($"Поток {Thread.CurrentThread.Name} прочитал в часть {part} {count} byte за {_processingStopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception)
            {
                Logger.Add($"Поток {Thread.CurrentThread.Name} - ошибка при чтении");
                Close();
                throw;
            }
            NextQueue?.Add(part);
            return true;
        }

        protected override void Close()
        {
            if (_archiveStream != null)
            {
                _archiveStream.Close();
                _archiveStream = null;
            }
        }
    }
}
