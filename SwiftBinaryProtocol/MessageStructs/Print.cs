using System;
using System.Text;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct Print
    {
        private string _message;

        public Print(byte[] data)
        {
            _message = Encoding.UTF8.GetString(data);
        }

        public string Message
        {
            get { return _message; }
        }

        public override string ToString()
        {
            return _message;
        }
    }
}
