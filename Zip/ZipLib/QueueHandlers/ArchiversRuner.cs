﻿using System.Threading;
using ZipLib.Loggers;
using ZipLib.Queues;
using ZipLib.Workers;

namespace ZipLib.QueueHandlers
{
    public class ArchiversRuner: QueueHandlerBase
    {
        private readonly ArchiversStatistic _statistic = new ArchiversStatistic();

        public ArchiversRuner(ILogger logger, IQueue sourceQueue, IQueue nextQueue)
            :base(logger, sourceQueue, nextQueue)
        {
            InnerThread = new Thread(this.Run) { Name = "ArchiversRuner" };
            InnerThread.Start();
        }

        private int _archiversCount;
        
        protected override bool ProcessPart(FilePart part)
        {
            _archiversCount++;
            var archiverName = "ArchiverN" + _archiversCount;
            var archiver = new Archiver(archiverName, Logger, _statistic, part, NextQueue);
            Logger.Add($"Поток {Thread.CurrentThread.Name} отдал part {part} archiver`у {archiverName}");

            archiver.Start();
            return true;
        }

        protected override void AddTotalToLog()
        {
            base.AddTotalToLog();
            Logger.Add($"Поток {Thread.CurrentThread.Name} среднее время архивирования одной части {_statistic.GetMiddleElapsedTime()} ms");
        }
    }
}