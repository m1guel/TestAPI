using System;
using System.Collections.Generic;
using System.Text;
using TestAPI.Domain.Types;

namespace TestAPI.Domain.Exceptions
{
    public class ErrorCodeFaultException : Exception
    {
        public ErrorCodeFaultException(ErrorCodeType errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public ErrorCodeFaultException(ErrorCodeType errorCode, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        public ErrorCodeType ErrorCode { get; private set; }
    }
}
