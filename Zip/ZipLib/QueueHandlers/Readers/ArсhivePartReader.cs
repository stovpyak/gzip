using System;
using System.Diagnostics;
using System.Threading;
using ZipLib.Decompress;
using ZipLib.Loggers;

namespace ZipLib.QueueHandlers.Readers
{
    public class ArсhivePartReader: PartrReaderBase
    {
        public ArсhivePartReader(ILogger logger) : base(logger)
        {
        }

        public int BufferSize { get; set; } = 10000;

        private ArhivePortion _portionForNextPart; // todo rename portionFromPrev
        private long _totalReadByte;

        public override bool ReadPart(FilePart part)
        {
            // одна часть архива - она полностью пойдет на декомпрессию
            var archivePart = new ArchivePart();
            ArhivePortion arhivePortion = null;
            while (archivePart != null)
            {
                if (_portionForNextPart != null)
                {
                    // используем порцию оставщуюся с предыдущей части 
                    arhivePortion = _portionForNextPart;
                    _portionForNextPart = null;
                }
                else
                {
                    // читаем порцию из файла 
                    var buffer = new byte[BufferSize];
                    var count = SourceStream.Read(buffer, 0, buffer.Length);
                    if (count > 0)
                    {
                        if (arhivePortion == null)
                            arhivePortion = new ArhivePortion(new BytesBuffer(buffer, 0, count - 1));
                        else
                        // предыдущая порция закончилась на части заголовка
                            arhivePortion.Append(new BytesBuffer(buffer, 0, count - 1));
                    }
                    else
                    {
                        // всё в part
                        part.Source = archivePart.ToArray();
                        part.IsLast = true;
                        return true;
                    }

                    _totalReadByte = _totalReadByte + count;
                    // прочитали всё - у части выставляем признак, что она последняя
                    if (_totalReadByte == SourceFileSize)
                    {
                        part.IsLast = true;
                        Logger.Add($"Поток {Thread.CurrentThread.Name} прочитал последнюю часть файла {part} ");
                    }
                }

                if (arhivePortion != null)
                {
                    // не нашли заголовков
                    if (!arhivePortion.IsExistsTitle)
                    {
                        if (archivePart.IsEmpty)
                            throw new FormatException("Часть ещё пустая, а в порции нет заголовка - неверный формат архива");

                        // всю прочитанную порцию архива в часть
                        archivePart.AppendAllPortion(arhivePortion);
                        Debug.Assert(arhivePortion.IsEmpty, "Всё извлекли из порции, а она всё равно не пустая");
                        arhivePortion = null;
                    }
                    else
                    {
                        // нашли заголовок полностью
                        if (arhivePortion.IsExistsAllTitle)
                        {
                            // заголовок в начале
                            if (arhivePortion.StartsWithTitle)
                            {
                                // часть архива пустая - нашли заголовок для текущей части
                                if (archivePart.IsEmpty)
                                {
                                    // записываем в часть все что до следеющего заголовка
                                    archivePart.AppendTitleAndDataBeforeNextTitle(arhivePortion);
                                    if (arhivePortion.IsNotEmpty)
                                        // а остаток нужно "припасти" для следующей part
                                        _portionForNextPart = arhivePortion;
                                    arhivePortion = null;
                                }
                                else
                                {
                                    // нашли заголовок для следующей части - значит текущая часть сформирована, а все что осталось в порции уже для следующей части
                                    // порцию нужно "припасти" для следующей part
                                    _portionForNextPart = arhivePortion;
                                    arhivePortion = null;
                                    // всё в part
                                    part.Source = archivePart.ToArray();
                                    archivePart = null;
                                }
                            }
                            else
                            {
                                // заголовок не в начале порции
                                if (archivePart.IsEmpty)
                                    throw new FormatException($"part {part}. archivePart ещё пустая, а в порции заголовок не в начале - неверный формат архива");

                                // добавляем в часть всё что до заголовка
                                archivePart.AppendDataBeforeTitle(arhivePortion);
                                // всё в part
                                part.Source = archivePart.ToArray();
                                // порцию нужно "припасти" для следующей part
                                _portionForNextPart = arhivePortion;
                                return true;
                            }
                        }
                        else
                        {
                            // нашли заголовок не полностью
                            // !!! окончить текущую часть не имеем права - не удостоверившись, что это заголовок
                            // поэтому здесь ничего не делаем, а выше прочитаем дополнительную порцию
                        }
                    }
                }
                else
                {
                    // архив закончился
                    if (archivePart.IsEmpty)
                        return false;
                    part.Source = archivePart.ToArray();
                    return true;
                }
            }
            return false;
        }
    }
}
