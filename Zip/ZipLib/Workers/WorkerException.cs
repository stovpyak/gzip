using System;

namespace ZipLib.Workers
{
    [Serializable]
    public class WorkerException: ApplicationException
    {
        public WorkerException() { }
        public WorkerException(string message) : base(message) { }
        public WorkerException(string message, Exception ex) : base(message, ex) { }
        
        protected WorkerException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext contex)
            : base(info, contex) { }
    }
}
