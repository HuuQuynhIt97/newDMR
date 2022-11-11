using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Helpers
{
    public class ResponseDetail<T>
    {
        public ResponseDetail()
        {
        }

        public ResponseDetail(T data, bool status, string message )
        {
            Data = data;
            Message = message;
            Status = status;
        }

        public T Data { get; set; }
        public string Message { get; set; }
        public bool Status { get; set; }
    }
}
