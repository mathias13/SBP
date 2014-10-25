using System;

namespace SwiftBinaryProtocol.Eventarguments
{
    public class SBPMessageEventArgs : EventArgs
    {
        protected int _senderId = -1;

        protected SBP_Enums.MessageTypes _messageType = SBP_Enums.MessageTypes.Unknown;

        protected object _data;

        public SBPMessageEventArgs(int senderId, SBP_Enums.MessageTypes messageType, object data)
        {
            _senderId = senderId;
            _messageType = messageType;
            _data = data;
        }

        public int SenderID
        {
            get { return _senderId; }
        }

        public SBP_Enums.MessageTypes MessageType
        {
            get { return _messageType; }
        }

        public object Data
        {
            get { return _data; }
        }
    }
}
