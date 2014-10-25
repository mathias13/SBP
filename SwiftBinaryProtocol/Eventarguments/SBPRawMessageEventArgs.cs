using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftBinaryProtocol.Eventarguments
{
    public class SBPRawMessageEventArgs : EventArgs
    {

        private byte[] _message;

        protected SBP_Enums.MessageTypes _messageType = SBP_Enums.MessageTypes.Unknown;

        public SBPRawMessageEventArgs(SBP_Enums.MessageTypes messageType, byte[] message)
        {
            _message = message;
            _messageType = messageType;
        }

        public SBP_Enums.MessageTypes MessageType
        {
            get { return _messageType; }
        }

        public byte[] Message
        {
            get { return _message; }
        }
    }
}
