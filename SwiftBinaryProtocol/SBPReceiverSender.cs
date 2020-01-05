using SwiftBinaryProtocol.Eventarguments;
using SwiftBinaryProtocol.MessageStructs;
using System;
using System.Collections.Generic;
using System.Net;
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

        private readonly IDictionary<SBP_Enums.MessageTypes, Type> MESSAGE_STRUCTS = new Dictionary<SBP_Enums.MessageTypes, Type>()
        {
            { SBP_Enums.MessageTypes.BASELINE_ECEF, typeof(BaselineECEF)},
            {SBP_Enums.MessageTypes.BASELINE_NED, typeof(BaselineNED)},
            {SBP_Enums.MessageTypes.BASELINE_HEADING, typeof(BaselineHeading)},
            {SBP_Enums.MessageTypes.BASELINE_ECEF_DEP, typeof(BaselineECEF_Dep)},
            {SBP_Enums.MessageTypes.BASELINE_NED_DEP, typeof(BaselineNED_Dep)},
            {SBP_Enums.MessageTypes.BASELINE_HEADING_DEP, typeof(BaselineHeading_Dep)},
            {SBP_Enums.MessageTypes.BASEPOS_LLH, typeof(BasePositionLLH)},
            {SBP_Enums.MessageTypes.BASEPOS_ECEF, typeof(BasePositionECEF)},
            {SBP_Enums.MessageTypes.DOPS, typeof(DilutionOfPrecision)},
            {SBP_Enums.MessageTypes.GPSTIME, typeof(GPSTime)},
            {SBP_Enums.MessageTypes.UTCTIME, typeof(UTCTime)},
            {SBP_Enums.MessageTypes.DOPS_DEP, typeof(DilutionOfPrecision_Dep)},
            {SBP_Enums.MessageTypes.GPSTIME_DEP, typeof(GPSTime_Dep)},
            {SBP_Enums.MessageTypes.HEARTBEAT, typeof(Heartbeat)},
            {SBP_Enums.MessageTypes.IAR_STATE, typeof(IARState)},
            {SBP_Enums.MessageTypes.OBSERVATION_DEP, typeof(Observation_Dep)},
            {SBP_Enums.MessageTypes.POS_ECEF_DEP, typeof(PosistionECEF_Dep)},
            {SBP_Enums.MessageTypes.POS_LLH_DEP, typeof(PositionLLH_Dep)},
            {SBP_Enums.MessageTypes.OBSERVATION, typeof(Observation)},
            {SBP_Enums.MessageTypes.POS_ECEF, typeof(PosistionECEF)},
            {SBP_Enums.MessageTypes.POS_LLH, typeof(PositionLLH)},
            {SBP_Enums.MessageTypes.STARTUP, typeof(Startup)},
            {SBP_Enums.MessageTypes.VEL_ECEF_DEP, typeof(VelocityECEF_Dep)},
            {SBP_Enums.MessageTypes.VEL_NED_DEP, typeof(VelocityNED_Dep)},
            {SBP_Enums.MessageTypes.VEL_ECEF, typeof(VelocityECEF)},
            {SBP_Enums.MessageTypes.VEL_NED, typeof(VelocityNED)},
            {SBP_Enums.MessageTypes.RESET_FILTERS, typeof(ResetFilters)},
            {SBP_Enums.MessageTypes.INIT_BASE, typeof(InitBase)},
            {SBP_Enums.MessageTypes.THREAD_STATE, typeof(SwiftBinaryProtocol.MessageStructs.ThreadState)},
            {SBP_Enums.MessageTypes.UART_STATE, typeof(UARTState)},
            {SBP_Enums.MessageTypes.ACQ_RESULT, typeof(AquisitionResult)},
            {SBP_Enums.MessageTypes.EXT_EVENT, typeof(ExternalEvent)},
            {SBP_Enums.MessageTypes.EPHEMERIS, typeof(Ephemeris)},
            {SBP_Enums.MessageTypes.MASK_SATELLITE, typeof(MaskSattelite)},            
            {SBP_Enums.MessageTypes.RESET, typeof(Reset)},
            {SBP_Enums.MessageTypes.TRACKING_STATE, typeof(TrackingState)},
            {SBP_Enums.MessageTypes.LOG, typeof(Log)}
        };
        
        #endregion

        #region Events

        public event EventHandler<SBPMessageEventArgs> ReceivedMessageEvent;
        
        #endregion

        #region ctor

        public SBPReceiverSender(string comPort, int baudrate, bool rtsCts) : base(comPort, baudrate, rtsCts)
        {
        }

        public SBPReceiverSender(IPAddress ipAdress, int tcpPort) : base(ipAdress, tcpPort)
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

            if(!_preambleFound)
            {
                while (_receivedBytes.Count > 0)
                {
                    if (_receivedBytes[0] == PREAMBLE)
                    {
                        _preambleFound = true;
                        _receivedBytes.RemoveAt(0);
                        break;
                    }
                    else
                        _receivedBytes.RemoveAt(0);
                }
            }

            if (_preambleFound)
            {
                if (!_message.MessageType.HasValue && _receivedBytes.Count > 1)
                {
                    _message.MessageType = BitConverter.ToUInt16(_receivedBytes.GetRange(0, 2).ToArray(), 0);
                    _receivedBytes.RemoveRange(0, 2);
                }

                if (!_message.SenderID.HasValue && _message.MessageType.HasValue && _receivedBytes.Count > 1)
                {
                    _message.SenderID = BitConverter.ToUInt16(_receivedBytes.GetRange(0, 2).ToArray(), 0);
                    _receivedBytes.RemoveRange(0, 2);
                }

                if (!_message.Length.HasValue && _message.SenderID.HasValue && _receivedBytes.Count > 0)
                {
                    _message.Length = _receivedBytes[0];
                    _receivedBytes.RemoveAt(0);
                }

                if (_message.Length.HasValue)
                {
                    if (_receivedBytes.Count > _message.Length.Value + 2)
                    {
                        _message.Payload.AddRange(_receivedBytes.GetRange(0, _message.Length.Value));
                        _receivedBytes.RemoveRange(0, _message.Length.Value);
                        _message.ReceicevedChecksum = BitConverter.ToUInt16(_receivedBytes.GetRange(0, 2).ToArray(), 0);
                        _receivedBytes.RemoveRange(0, 2);
                        if (_message.ValidateCheckSum())
                        {
                            SBP_Enums.MessageTypes messageTypeEnum = SBP_Enums.MessageTypes.Unknown;
                            if (Enum.IsDefined(typeof(SBP_Enums.MessageTypes), (int)_message.MessageType))
                                messageTypeEnum = (SBP_Enums.MessageTypes)(int)_message.MessageType;

                            if (MESSAGE_STRUCTS.ContainsKey(messageTypeEnum))
                            {
                                object messageData = Activator.CreateInstance(MESSAGE_STRUCTS[messageTypeEnum], _message.Payload.ToArray());                                
                                lock (_syncobject)
                                    _messageQueue.Enqueue(new SBPMessageEventArgs((int)_message.SenderID.Value, messageTypeEnum, messageData));
                            }
                        }
                        else
                        {
                            lock (_syncobject)
                                _readExceptionQueue.Enqueue(new SBPReadExceptionEventArgs(new Exception("CRC not valid")));
                        }


                        _message = new SBPReceiveMessage();
                        _preambleFound = false;
                    }
                }
            }
            
        }

        protected override bool InvokeThreadExecute()
        {
            SBPMessageEventArgs sendMessage = null;
            if (_messageQueue.Count > 0)
                lock (_syncobject)
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
