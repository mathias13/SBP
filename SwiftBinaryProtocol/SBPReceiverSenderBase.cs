﻿using SwiftBinaryProtocol.Eventarguments;
using System;
using System.Collections.Generic;
using PInvokeSerialPort;
using System.Threading;

namespace SwiftBinaryProtocol
{
    public abstract class SBPReceiverSenderBase : IDisposable
    {
        #region Private Variables
        
        protected object _syncobject = new object();

        private bool _receiveSendThreadStopped = false;

        private Thread _receiveSendThread;

        protected Queue<byte[]> _sendMessageQueue = new Queue<byte[]>();

        private bool _invokeThreadStop = false;

        private Thread _invokeThread;

        protected Queue<SBPSendExceptionEventArgs> _sendExceptionQueue = new Queue<SBPSendExceptionEventArgs>();

        protected Queue<SBPReadExceptionEventArgs> _readExceptionQueue = new Queue<SBPReadExceptionEventArgs>();

        private string _comPort = String.Empty;

        private int _baudRate = 19200;

        protected Queue<byte> _receivedBytes;
        
        public const byte PREAMBLE = 0x55;

        public const int MAX_BYTE_BLOCK_SIZE = 4096;

        #endregion

        #region Events
        
        public event EventHandler<SBPSendExceptionEventArgs> SendExeceptionEvent;

        public event EventHandler<SBPReadExceptionEventArgs> ReadExceptionEvent;

        #endregion

        #region ctor

        public SBPReceiverSenderBase(string comPort, int baudrate)
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

        private void ReceiveSendThread()
        {
            bool restart = false;
            Thread.Sleep(1000);
            _receivedBytes = new Queue<byte>();
            
            while(!_receiveSendThreadStopped)
            {
                try
                {

                    using(SerialPort serialPort = new SerialPort(_comPort, _baudRate))
                    {
                        serialPort.DataBits = 8;
                        serialPort.Parity = Parity.None;
                        serialPort.StopBits = StopBits.one;
                        serialPort.RxQueue = 16384;
                        serialPort.UseDtr = HsOutput.Handshake;
                        serialPort.UseRts = HsOutput.Handshake;
                        serialPort.Open();
                        serialPort.DataReceived += serialPort_DataReceived;
                        while(!_receiveSendThreadStopped)
                        {
                            if(_sendMessageQueue.Count > 0)
                            { 
                                byte[] messageToSend;
                                lock (_syncobject)
                                    messageToSend = _sendMessageQueue.Dequeue();

                                try
                                {
                                    serialPort.Write(messageToSend);
                                    serialPort.Flush();
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

                            ProcessReading(restart);

                            Thread.Sleep(4);
                        }

                    }
                }
                catch(Exception e)
                {
                    _receivedBytes.Clear();
                    restart = true;
                    lock (_syncobject)
                        _readExceptionQueue.Enqueue(new SBPReadExceptionEventArgs(e));

                    Thread.Sleep(1000);
                }
            }
        }

        private void serialPort_DataReceived(byte[] obj)
        {
            lock (_syncobject)
                foreach(byte Byte in obj)
                    _receivedBytes.Enqueue(Byte);
        }

        protected virtual void ProcessReading(bool restart)
        {
        }

        private void InvokeThread()
        {
            while(!_invokeThreadStop)
            {
                bool somethingToDo = false;
                SBPSendExceptionEventArgs sendException = null;
                SBPReadExceptionEventArgs readException = null;
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
                }

                if(somethingToDo)
                {
                    if (sendException != null)
                        OnSendException(sendException);
                    if (readException != null)
                        OnReadException(readException);
                }
                else if (!InvokeThreadExecute())
                    Thread.Sleep(10);

            }
        }

        protected virtual bool InvokeThreadExecute()
        {
            return false;
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
        
        #endregion

        #region Public Methods

        public void Dispose()
        {
            _receiveSendThreadStopped = true;
            _invokeThreadStop = true;
            _receiveSendThread.Join();
            _invokeThread.Join();
        }
        
        #endregion
    }
}
