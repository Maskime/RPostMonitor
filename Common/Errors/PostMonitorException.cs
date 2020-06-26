// ************************************************************************************************
// 
//  © 2019       General Electric Company
// 
//  Description  See class summary below.
// 
//  History      See source code control system.
// 
// ************************************************************************************************

using System;

namespace Common.Errors
{
    public class PostMonitorException:Exception
    {
        public PostMonitorException(string? message) : base(message)
        {
        }

        public PostMonitorException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
