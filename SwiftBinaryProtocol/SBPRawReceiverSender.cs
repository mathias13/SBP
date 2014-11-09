using SwiftBinaryProtocol.Eventarguments;
using SwiftBinaryProtocol.MessageStructs;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SwiftBinaryProtocol
{
    public class SBPRawReceiverSender : SBPReceiverSenderBase
    {
        #region Private Variable

        private Queue<byte[]> _messageQueue = new Queue<byte[]>();

        private bool _preambleFound = false;

        private List<byte> _messageBytes = new List<byte>();

        #endregion

        #region Events

        public event EventHandler<SBPRawMessageEventArgs> ReceivedRawMessageEvent;
    
        #endregion

        #region ctor

        public SBPRawReceiverSender(string comport, int baudrate)
            : base(comport, baudrate)
        { }

        #endregion

        #region Protected Methods

        protected override void ProcessReading(bool restart)
        {
            if(restart)
            {
                _preambleFound = false;
                _messageBytes.Clear();
            }

            //We need at least two bytes every time to interpret fields without problem
            while (_receivedBytes.Count > 0)
            {
                if (_preambleFound)
                {
                    if (_messageBytes.Count == 0)
                        _messageBytes.Add(PREAMBLE);
                    else if(_messageBytes.Count < 6)
                        _messageBytes.Add(_receivedBytes.Dequeue());
                    else if(_messageBytes.Count < ((int)_messageBytes[5] + 8))
                        _messageBytes.Add(_receivedBytes.Dequeue());
                    else
                    {
                        List<byte> crcBytes = new List<byte>();
                        for (int i = 1; i < _messageBytes.Count - 2; i++)
                            crcBytes.Add(_messageBytes[i]);

                        ushort crc = Crc16CcittKermit.ComputeChecksum(crcBytes.ToArray());
                        byte[] crcSumBytes = new byte[2] { _messageBytes[_messageBytes.Count - 2], _messageBytes[_messageBytes.Count - 1] };
                        ushort crcInMessage = BitConverter.ToUInt16(crcSumBytes, 0);
                        if (crc == crcInMessage)
                            _messageQueue.Enqueue(_messageBytes.ToArray());
                        else
                        {
                            lock (_syncobject)
                                _readExceptionQueue.Enqueue(new SBPReadExceptionEventArgs(new Exception("CRC not valid")));
                        }
                        _messageBytes.Clear();
                        _preambleFound = false;
                    }
                }
                else
                    if (_receivedBytes.Dequeue() == PREAMBLE)
                        _preambleFound = true;
            }
        }

        protected override bool InvokeThreadExecute()
        {
            SBPRawMessageEventArgs sendMessage = null;
            lock (_syncobject)
            {
                if (_messageQueue.Count > 0)
                {
                    byte[] message = _messageQueue.Dequeue();
                    int messageType = BitConverter.ToUInt16(new byte[]{message[1], message[2]},0);
                    SBP_Enums.MessageTypes messageTypeEnum = SBP_Enums.MessageTypes.Unknown;
                    if (Enum.IsDefined(typeof(SBP_Enums.MessageTypes), (int)messageType))
                        messageTypeEnum = (SBP_Enums.MessageTypes)(int)messageType;

                    sendMessage = new SBPRawMessageEventArgs(messageTypeEnum, message);
                }
            }

            if (sendMessage != null)
            {
                OnReceivedRawMessage(sendMessage);
                return true;
            }
            else
                return false;
        }

        protected void OnReceivedRawMessage(SBPRawMessageEventArgs e)
        {
            if (ReceivedRawMessageEvent != null)
                ReceivedRawMessageEvent.Invoke(this, e);
        }

        #endregion

        #region Public Methods

        public void SendMessage(byte[] data)
        {
            lock (_syncobject)
                _sendMessageQueue.Enqueue(data);
        }

        #endregion
    }
}
