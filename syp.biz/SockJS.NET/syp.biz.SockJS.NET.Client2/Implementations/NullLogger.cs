﻿using syp.biz.SockJS.NET.Client2.Interfaces;

namespace syp.biz.SockJS.NET.Client2.Implementations
{
    internal class NullLogger : ILogger
    {
        #region Implementation of ILogger
        public void Debug(string message) { }
        public void Info(string message) { }
        public void Error(string message) { }
        #endregion Implementation of ILogger
    }
}
