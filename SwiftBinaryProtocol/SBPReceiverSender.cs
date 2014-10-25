using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.IO.Ports;
using SwiftBinaryProtocol.MessageStructs;
using SwiftBinaryProtocol.Eventarguments;

namespace SwiftBinaryProtocol
{
    internal class SBPReceiveMessage
    {
        public ushort? MessageType;

        public ushort? SenderID;

        public byte? Length;

        public byte[] Payload;

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
            Payload = new byte[0];
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

    public class SBPReceiverSender : IDisposable
    {
        #region Private Variables
        
        protected object _syncobject = new object();

        protected bool _receiveSendThreadStopped = false;

        protected Thread _receiveSendThread;

        private Queue<byte[]> _sendMessageQueue = new Queue<byte[]>();

        protected bool _invokeThreadStopped = false;

        protected Thread _invokeThread;

        protected Queue<SBPSendExceptionEventArgs> _sendExceptionQueue = new Queue<SBPSendExceptionEventArgs>();

        protected Queue<SBPReadExceptionEventArgs> _readExceptionQueue = new Queue<SBPReadExceptionEventArgs>();

        private Queue<SBPMessageEventArgs> _messageQueue = new Queue<SBPMessageEventArgs>();

        protected string _comPort = String.Empty;

        protected int _baudRate = 19200;

        public const byte PREAMBLE = 0x55;

        #endregion

        #region Events

        public event EventHandler<SBPMessageEventArgs> ReceivedMessageEvent;

        public event EventHandler<SBPSendExceptionEventArgs> SendExeceptionEvent;

        public event EventHandler<SBPReadExceptionEventArgs> ReadExceptionEvent;

        #endregion

        #region ctor

        public SBPReceiverSender(string comPort, int baudrate)
        {
            _comPort = comPort;
            _baudRate = baudrate;

            _receiveSendThread = new Thread(new ThreadStart(ReceiveSendThread));
            _receiveSendThread.Start();

            _invokeThread = new Thread(new ThreadStart(InvokeThread));
            _invokeThread.Start();
        }

        #endregion

        #region Protected Methods

        protected virtual void ReceiveSendThread()
        {
            Thread.Sleep(1000);
            bool preambleFound = false;
            SBPReceiveMessage message = new SBPReceiveMessage();
            while(!_receiveSendThreadStopped)
            {
                try
                {

                    using(SerialPort serialPort = new SerialPort(_comPort, _baudRate, Parity.None, 8, StopBits.One))
                    {
                        serialPort.DtrEnable = true;
                        serialPort.RtsEnable = true;
                        serialPort.Handshake = Handshake.RequestToSend;
                        serialPort.Open();
                        preambleFound = false;
                        while(!_receiveSendThreadStopped)
                        {
                            if(_sendMessageQueue.Count > 0)
                            { 
                                byte[] messageToSend;
                                lock (_syncobject)
                                    messageToSend = _sendMessageQueue.Dequeue();

                                try
                                {
                                    serialPort.Write(messageToSend, 0, messageToSend.Length);
                                }
                                catch (Exception e)
                                {
                                    lock (_syncobject)
                                    {
                                        if (_sendMessageQueue.Count > 100)
                                            _sendMessageQueue.Clear();
                                        _sendExceptionQueue.Enqueue(new SBPSendExceptionEventArgs(e));
                                    }
                                }
                            }

                            int neededBytes = 2;
                            while (serialPort.BytesToRead >= neededBytes)
                            {
                                if(preambleFound)
                                {
                                    if (!message.MessageType.HasValue)
                                    {
                                        byte[] messageTypeBytes = new byte[2] { (byte)serialPort.ReadByte(), (byte)serialPort.ReadByte() };
                                        message.MessageType = BitConverter.ToUInt16(messageTypeBytes, 0);
                                    }
                                    else if (!message.SenderID.HasValue)
                                    {
                                        byte[] senderBytes = new byte[2] { (byte)serialPort.ReadByte(), (byte)serialPort.ReadByte() };
                                        message.SenderID = BitConverter.ToUInt16(senderBytes, 0);
                                    }
                                    else if (!message.Length.HasValue)
                                    {
                                        message.Length = (byte)serialPort.ReadByte();
                                        neededBytes = (int)message.Length.Value;
                                    }
                                    else if (serialPort.BytesToRead >= (int)message.Length.Value)
                                    {
                                        message.Payload = new byte[(int)message.Length.Value];
                                        serialPort.Read(message.Payload, 0, (int)message.Length.Value);
                                    }
                                    else if (message.Payload.Length == (int)message.Length.Value)
                                    {
                                        byte[] crcBytes = new byte[2] { (byte)serialPort.ReadByte(), (byte)serialPort.ReadByte() };
                                        message.ReceicevedChecksum = BitConverter.ToUInt16(crcBytes, 0);
                                        if (message.ValidateCheckSum())
                                        {
                                            SBP_Enums.MessageTypes messageTypeEnum = SBP_Enums.MessageTypes.Unknown;
                                            if (Enum.IsDefined(typeof(SBP_Enums.MessageTypes), (int)message.MessageType))
                                                messageTypeEnum = (SBP_Enums.MessageTypes)(int)message.MessageType;

                                            object messageData = new object();
                                            switch (messageTypeEnum)
                                            {
                                                case SBP_Enums.MessageTypes.BASELINE_ECEF:
                                                    messageData = new BaselineECEF(message.Payload);
                                                    break;

                                                case SBP_Enums.MessageTypes.BASELINE_NED:
                                                    messageData = new BaselineNED(message.Payload);
                                                    break;

                                                case SBP_Enums.MessageTypes.DOPS:
                                                    messageData = new DilutionOfPrecision(message.Payload);
                                                    break;

                                                case SBP_Enums.MessageTypes.GPSTIME:
                                                    messageData = new GPSTime(message.Payload);
                                                    break;

                                                case SBP_Enums.MessageTypes.HEARTBEAT:
                                                    messageData = new Heartbeat(message.Payload);
                                                    break;

                                                case SBP_Enums.MessageTypes.POS_ECEF:
                                                    messageData = new PosistionECEF(message.Payload);
                                                    break;

                                                case SBP_Enums.MessageTypes.POS_LLH:
                                                    messageData = new PositionLLH(message.Payload);
                                                    break;

                                                case SBP_Enums.MessageTypes.VEL_ECEF:
                                                    messageData = new VelocityECEF(message.Payload);
                                                    break;

                                                case SBP_Enums.MessageTypes.VEL_NED:
                                                    messageData = new VelocityNED(message.Payload);
                                                    break;

                                                case SBP_Enums.MessageTypes.OBS:
                                                    messageData = new Observation(message.Payload);
                                                    break;

                                                case SBP_Enums.MessageTypes.OBS_HDR:
                                                    messageData = new ObservationHeader(message.Payload);
                                                    break;

                                                case SBP_Enums.MessageTypes.IAR_STATE:
                                                    messageData = new IARState(message.Payload);
                                                    break;

                                                case SBP_Enums.MessageTypes.PRINT:
                                                    messageData = new Print(message.Payload);
                                                    break;
                                            }

                                            lock (_syncobject)
                                                _messageQueue.Enqueue(new SBPMessageEventArgs((int)message.SenderID.Value, messageTypeEnum, messageData));
                                        }
                                        else
                                        {
                                            lock (_syncobject)
                                                _readExceptionQueue.Enqueue(new SBPReadExceptionEventArgs(new Exception("CRC not valid")));
                                        }

                                        message = new SBPReceiveMessage();
                                        preambleFound = false;
                                        break;
                                    }
                                }
                                else
                                    if ((byte)serialPort.ReadByte() == PREAMBLE)
                                        preambleFound = true;

                            }

                            if (serialPort.BytesToRead > 3000)
                                Thread.Sleep(0);

                            if (serialPort.BytesToRead < 10)
                                Thread.Sleep(1);
                        }

                    }
                }
                catch(Exception e)
                {
                    preambleFound = false;
                    message = new SBPReceiveMessage();
                    Utilities.Log.Log.Error(e);
                    lock (_syncobject)
                        _readExceptionQueue.Enqueue(new SBPReadExceptionEventArgs(e));
                }
            }
        }

        protected virtual void InvokeThread()
        {
            while(!_invokeThreadStopped)
            {
                bool somethingToDo = false;
                SBPSendExceptionEventArgs sendException = null;
                SBPReadExceptionEventArgs readException = null;
                SBPMessageEventArgs message = null;
                lock(_syncobject)
                {
                    if (_sendExceptionQueue.Count > 0)
                    {
                        sendException = _sendExceptionQueue.Dequeue();
                        somethingToDo = true;
                    }

                    if (_readExceptionQueue.Count > 0)
                    {
                        readException = _readExceptionQueue.Dequeue();
                        somethingToDo = true;
                    }

                    if (_messageQueue.Count > 0)
                    {
                        message = _messageQueue.Dequeue();
                        somethingToDo = true;
                    }
                }

                if(somethingToDo)
                {
                    if (sendException != null)
                        OnSendException(sendException);
                    if (readException != null)
                        OnReadException(readException);
                    if (message != null)
                        OnReceivedMessage(message);
                }
                else
                    Thread.Sleep(10);

            }
        }

        protected void OnSendException(SBPSendExceptionEventArgs e)
        {
            if(SendExeceptionEvent!= null)
                SendExeceptionEvent.Invoke(this, e);
        }

        protected void OnReadException(SBPReadExceptionEventArgs e)
        {
            if (ReadExceptionEvent != null)
                ReadExceptionEvent.Invoke(this, e);
        }

        protected void OnReceivedMessage(SBPMessageEventArgs e)
        {
            if (ReceivedMessageEvent != null)
                ReceivedMessageEvent.Invoke(this, e);
        }

        #endregion

        #region Public Methods

        public void Dispose()
        {
            _receiveSendThreadStopped = true;
            _invokeThreadStopped = true;
            _receiveSendThread.Join();
            _invokeThread.Join();
        }

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
