using System;
using System.Collections.Generic;
using System.Text;

namespace TestAPI.Domain.Types
{
    public enum ErrorCodeType
    {
        //The first 1000 error codes are reserved for specific HTTP errors
        Unknown = 1000,
        EmailRequired = 1001,
        PasswordRequired = 1002,
        ShortPassword = 1003,
        FirstNameRequired = 1004,
        EmailAlreadyExists = 1005,
        LastNameRequired = 1006,
    }
}
