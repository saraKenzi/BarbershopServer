using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarbershopEntities
{
    public class OperationResult<T>
    {
        public bool IsSuccessful { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public T MyObject { get; set; }

        
        public OperationResult(bool isSuccessful, int statusCode, T myObject, string message= "isSuccessful")
        {
            IsSuccessful = isSuccessful;
            StatusCode = statusCode;
            MyObject = myObject;
            Message = message;
        }
        public OperationResult(bool isSuccessful, int statusCode, string message)
        {
            IsSuccessful = isSuccessful;
            StatusCode = statusCode;
            MyObject = default;
            Message = message;
        }
    }
}
