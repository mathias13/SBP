using SwiftBinaryProtocol.Eventarguments;
using SwiftBinaryProtocol.MessageStructs;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace SwiftBinaryProtocol
{
    public class SBPRawReceiverSender : SBPReceiverSender
    {
        #region Private Variable

        private Queue<byte[]> _sendMessageQueue = new Queue<byte[]>();

        private Queue<byte[]> _messageQueue = new Queue<byte[]>();

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

        protected override void ReceiveSendThread()
        {
            Thread.Sleep(1000);
            bool preamableFound = false;
            List<byte> messageBytes = new List<byte>();
            while (!_receiveSendThreadStopped)
            {
                try
                {

                    using (SerialPort serialPort = new SerialPort(_comPort, _baudRate, Parity.None, 8, StopBits.One))
                    {
                        serialPort.ReadBufferSize = 65536;
                        serialPort.Open();
                        preamableFound = false;
                        while (!_receiveSendThreadStopped)
                        {
                            if (_sendMessageQueue.Count > 0)
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
                                        _sendExceptionQueue.Enqueue(new SBPSendExceptionEventArgs(e));
                                }
                            }

                            //We need at least two bytes every time to interpret fields without problem
                            while (serialPort.BytesToRead > 0)
                            {
                                if (preamableFound)
                                {
                                    if (messageBytes.Count == 0)
                                        messageBytes.Add(PREAMBLE);

                                    while (messageBytes.Count < 6 && serialPort.BytesToRead > 0)
                                        messageBytes.Add((byte)serialPort.ReadByte());
                                   
                                    while (messageBytes.Count < ((int)messageBytes[5] + 8) && messageBytes.Count >= 6 && serialPort.BytesToRead > 0)
                                        messageBytes.Add((byte)serialPort.ReadByte());

                                    if (messageBytes.Count == ((int)messageBytes[5] + 8))
                                    {
                                        List<byte> crcBytes = new List<byte>();
                                        for (int i = 1; i < messageBytes.Count - 2; i++)
                                            crcBytes.Add(messageBytes[i]);

                                        ushort crc = Crc16CcittKermit.ComputeChecksum(crcBytes.ToArray());
                                        byte[] crcSumBytes = new byte[2] { messageBytes[messageBytes.Count - 2], messageBytes[messageBytes.Count - 1] };
                                        ushort crcInMessage = BitConverter.ToUInt16(crcSumBytes, 0);
                                        if (crc == crcInMessage)
                                            _messageQueue.Enqueue(messageBytes.ToArray());
                                        else
                                        {
                                            lock (_syncobject)
                                                _readExceptionQueue.Enqueue(new SBPReadExceptionEventArgs(new Exception("CRC not valid")));
                                        }
                                        messageBytes.Clear();
                                        preamableFound = false;
                                    }
                                }
                                else
                                    if ((byte)serialPort.ReadByte() == PREAMBLE)
                                        preamableFound = true;
                            }

                            if (serialPort.BytesToRead < 512)
                                Thread.Sleep(2);
                        }

                    }
                }
                catch (Exception e)
                {
                    lock (_syncobject)
                        _readExceptionQueue.Enqueue(new SBPReadExceptionEventArgs(e));
                }
            }
        }

        protected override void InvokeThread()
        {
            while (!_invokeThreadStop)
            {
                bool somethingToDo = false;
                SBPSendExceptionEventArgs sendException = null;
                SBPReadExceptionEventArgs readException = null;
                SBPRawMessageEventArgs message = null;
                lock (_syncobject)
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
                        byte[] messageBytes = _messageQueue.Dequeue();
                        SBP_Enums.MessageTypes messageTypeEnum = SBP_Enums.MessageTypes.Unknown;
                        ushort messageType = BitConverter.ToUInt16(new byte[] { messageBytes[1], messageBytes[2] }, 0);
                        if (Enum.IsDefined(typeof(SBP_Enums.MessageTypes), (int)messageType))
                            messageTypeEnum = (SBP_Enums.MessageTypes)(int)(int)messageType;
                        message = new SBPRawMessageEventArgs(messageTypeEnum, messageBytes);
                        somethingToDo = true;
                    }
                }

                if (somethingToDo)
                {
                    if (sendException != null)
                        OnSendException(sendException);

                    if (readException != null)
                        OnReadException(readException);

                    if (message != null)
                        OnReceivedRawMessage(message);
                }
                else
                    Thread.Sleep(10);

            }
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
