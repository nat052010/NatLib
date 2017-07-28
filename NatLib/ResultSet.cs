using System;
using System.Runtime.Serialization;

namespace NatLib
{
    [DataContract]
    public class ResultSet: IDisposable
    {
        private bool _disposed = false;

        [DataMember]
        public object Result { get; set; }

        [DataMember]
        public int Status { get; set; }

        [DataMember]
        public Error Error { get; set; }

        public ResultSet()
        {
            Status = 1;
        }

        public void Invalid()
        {
            Status = 0;
        }

        public void ErrAssign(Exception ex)
        {
            Error = new Error
            {
                Message = ex.Message,
                Number = ex.HResult
            };

            ex.Message.Log();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {

            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ResultSet()
        {
            Dispose(false);
        }
    }
}
