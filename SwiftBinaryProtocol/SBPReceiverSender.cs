using SwiftBinaryProtocol.Eventarguments;
using SwiftBinaryProtocol.MessageStructs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SwiftBinaryProtocol
{
    internal class SBPReceiveMessage
    {
        public ushort? MessageType;

        public ushort? SenderID;

        public byte? Length;

        public List<byte> Payload;

        public ushort? ReceicevedChecksum;

        public bool ValidateCheckSum()
        {
            if (ReceicevedChecksum.HasValue)
            {
                List<byte> bytes = new List<byte>();
                bytes.AddRange(BitConverter.GetBytes((ushort)(int)MessageType.Value));
                bytes.AddRange(BitConverter.GetBytes(SenderID.Value));
                bytes.Add(Length.Value);
                bytes.AddRange(Payload);
                return Crc16CcittKermit.ComputeChecksum(bytes.ToArray()) == ReceicevedChecksum.Value;
            }
            else
                return false;
        }

        public SBPReceiveMessage()
        {
            Payload = new List<byte>();
        }
    }
    
    internal struct SBPSendMessage
    {
        private SBP_Enums.MessageTypes _messageType;

        private int _senderID;

        private IPayload _payload;

        public SBPSendMessage(SBP_Enums.MessageTypes messageType, int senderID, IPayload payload)
        {
            _messageType = messageType;
            _senderID = senderID;
            _payload = payload;
        }

        public SBP_Enums.MessageTypes MessageTypeEnum
        {
            get { return _messageType; }
        }

        public ushort MessageType
        {
            get { return (ushort)(int)_messageType; }
        }

        public ushort SenderID
        {
            get { return (ushort)_senderID; }
        }

        public IPayload Payload
        {
            get { return _payload; }
        }

        public byte Length
        {
            get { return (byte)_payload.Data.Length; }
        }

        public ushort GetChecksum()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(MessageType));
            bytes.AddRange(BitConverter.GetBytes(SenderID));
            bytes.Add(Length);
            bytes.AddRange(Payload.Data);
            return Crc16CcittKermit.ComputeChecksum(bytes.ToArray());
        }
    }

    public class SBPReceiverSender : SBPReceiverSenderBase
    {
        #region Private Variables
        
        private Queue<SBPMessageEventArgs> _messageQueue = new Queue<SBPMessageEventArgs>();

        private bool _preambleFound = false;

        private SBPReceiveMessage _message = new SBPReceiveMessage();

        #endregion

        #region Events

        public event EventHandler<SBPMessageEventArgs> ReceivedMessageEvent;
        
        #endregion

        #region ctor

        public SBPReceiverSender(string comPort, int baudrate):base(comPort, baudrate)
        {
        }

        #endregion

        #region Protected Methods

        protected override void ProcessReading(bool restart)
        {
            if(restart)
            {
                _preambleFound = false;
                _message = new SBPReceiveMessage();
            }

            lock (_syncobject)
            {
                //We need at least two bytes to read fields properly
                while (_receivedBytes.Count > 1)
                {
                    if (_preambleFound)
                    {
                        if (!_message.MessageType.HasValue)
                        {
                            byte[] messageTypeBytes = new byte[2] { _receivedBytes.Dequeue(), _receivedBytes.Dequeue() };
                            _message.MessageType = BitConverter.ToUInt16(messageTypeBytes, 0);
                        }
                        else if (!_message.SenderID.HasValue)
                        {
                            byte[] senderBytes = new byte[2] { _receivedBytes.Dequeue(), _receivedBytes.Dequeue() };
                            _message.SenderID = BitConverter.ToUInt16(senderBytes, 0);
                        }
                        else if (!_message.Length.HasValue)
                            _message.Length = _receivedBytes.Dequeue();
                        else if (_message.Payload.Count < _message.Length.Value)
                            _message.Payload.Add(_receivedBytes.Dequeue());
                        else
                        {
                            byte[] crcBytes = new byte[2] { _receivedBytes.Dequeue(), _receivedBytes.Dequeue() };
                            _message.ReceicevedChecksum = BitConverter.ToUInt16(crcBytes, 0);
                            if (_message.ValidateCheckSum())
                            {
                                SBP_Enums.MessageTypes messageTypeEnum = SBP_Enums.MessageTypes.Unknown;
                                if (Enum.IsDefined(typeof(SBP_Enums.MessageTypes), (int)_message.MessageType))
                                    messageTypeEnum = (SBP_Enums.MessageTypes)(int)_message.MessageType;

                                object messageData = new object();
                                switch (messageTypeEnum)
                                {
                                    case SBP_Enums.MessageTypes.BASELINE_ECEF:
                                        messageData = new BaselineECEF(_message.Payload.ToArray());
                                        break;

                                    case SBP_Enums.MessageTypes.BASELINE_NED:
                                        messageData = new BaselineNED(_message.Payload.ToArray());
                                        break;

                                    case SBP_Enums.MessageTypes.DOPS:
                                        messageData = new DilutionOfPrecision(_message.Payload.ToArray());
                                        break;

                                    case SBP_Enums.MessageTypes.GPSTIME:
                                        messageData = new GPSTime(_message.Payload.ToArray());
                                        break;

                                    case SBP_Enums.MessageTypes.HEARTBEAT:
                                        messageData = new Heartbeat(_message.Payload.ToArray());
                                        break;

                                    case SBP_Enums.MessageTypes.POS_ECEF:
                                        messageData = new PosistionECEF(_message.Payload.ToArray());
                                        break;

                                    case SBP_Enums.MessageTypes.POS_LLH:
                                        messageData = new PositionLLH(_message.Payload.ToArray());
                                        break;

                                    case SBP_Enums.MessageTypes.VEL_ECEF:
                                        messageData = new VelocityECEF(_message.Payload.ToArray());
                                        break;

                                    case SBP_Enums.MessageTypes.VEL_NED:
                                        messageData = new VelocityNED(_message.Payload.ToArray());
                                        break;

                                    case SBP_Enums.MessageTypes.OBS:
                                        messageData = new Observation(_message.Payload.ToArray());
                                        break;

                                    case SBP_Enums.MessageTypes.OBS_HDR:
                                        messageData = new ObservationHeader(_message.Payload.ToArray());
                                        break;

                                    case SBP_Enums.MessageTypes.IAR_STATE:
                                        messageData = new IARState(_message.Payload.ToArray());
                                        break;

                                    case SBP_Enums.MessageTypes.PRINT:
                                        messageData = new Print(_message.Payload.ToArray());
                                        break;
                                }

                                lock (_syncobject)
                                    _messageQueue.Enqueue(new SBPMessageEventArgs((int)_message.SenderID.Value, messageTypeEnum, messageData));
                            }
                            else
                            {
                                lock (_syncobject)
                                    _readExceptionQueue.Enqueue(new SBPReadExceptionEventArgs(new Exception("CRC not valid")));
                            }

                            _message = new SBPReceiveMessage();
                            _preambleFound = false;
                            break;
                        }
                    }
                    else
                        if (_receivedBytes.Dequeue() == PREAMBLE)
                            _preambleFound = true;

                }
            }        
        }

        protected override bool InvokeThreadExecute()
        {
            SBPMessageEventArgs sendMessage = null;
            lock (_syncobject)
                if (_messageQueue.Count > 0)
                    sendMessage = _messageQueue.Dequeue();

            if (sendMessage != null)
            {
                OnReceivedMessage(sendMessage);
                return true;
            }
            else
                return false;
        }

        protected void OnReceivedMessage(SBPMessageEventArgs e)
        {
            if (ReceivedMessageEvent != null)
                ReceivedMessageEvent.Invoke(this, e);
        }

        #endregion

        #region Public Methods

        public void SendMessage(SBP_Enums.MessageTypes messageType, int senderID, object data)
        {
            if (data is IPayload)
            {
                if (messageType == SBP_Enums.MessageTypes.Unknown)
                    throw new Exception("Parameter messageType can't be unknown");

                SBPSendMessage messageToSend = new SBPSendMessage(messageType, senderID, (IPayload)data);
                List<Byte> bytes = new List<byte>();
                bytes.Add(PREAMBLE);
                bytes.AddRange(BitConverter.GetBytes(messageToSend.MessageType));
                bytes.AddRange(BitConverter.GetBytes(messageToSend.SenderID));
                bytes.Add(messageToSend.Length);
                bytes.AddRange(messageToSend.Payload.Data);
                bytes.AddRange(BitConverter.GetBytes(messageToSend.GetChecksum()));
                lock (_syncobject)
                    _sendMessageQueue.Enqueue(bytes.ToArray());
            }
            else
                throw new Exception("Parameter data has no IPayload interface");
        }

        public void SendMessage(byte[] data)
        {
            lock (_syncobject)
                _sendMessageQueue.Enqueue(data);
        }

        #endregion
    }
}
