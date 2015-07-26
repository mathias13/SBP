using System;
using System.Text;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct Log
    {
        public enum LogLevel
        {            
            DEBUG = 0,
            INFO = 1,
            WARN = 2,
            ERROR = 3,
            UNKNOWN = -1
        }

        private LogLevel _logLevel;

        private string _message;

        public Log(byte[] data)
        {
            _logLevel = LogLevel.UNKNOWN;
            int logLevel = (int)data[0];
            if(Enum.IsDefined(typeof(LogLevel), logLevel))
                _logLevel = (LogLevel)logLevel;
            _message = Encoding.UTF8.GetString(data, 1, data.Length - 1);
        }

        public LogLevel Level
        {
            get { return _logLevel; }
        }

        public string Message
        {
            get { return _message; }
        }

        public override string ToString()
        {
            return _logLevel.ToString() + ": " + _message;
        }
    }
}
