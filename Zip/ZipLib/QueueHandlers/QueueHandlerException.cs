using System;

namespace ZipLib.QueueHandlers
{
    [Serializable]
    public class QueueHandlerException: ApplicationException
    {
        public QueueHandlerException() { }
        public QueueHandlerException(string message) : base(message) { }
        public QueueHandlerException(string message, Exception ex) : base(message, ex) { }
        
        protected QueueHandlerException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext contex)
            : base(info, contex) { }
    }
}
