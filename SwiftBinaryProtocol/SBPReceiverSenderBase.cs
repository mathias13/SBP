using SwiftBinaryProtocol.Eventarguments;
using System;
using System.Collections.Generic;
using SwiftBinaryProtocol.Win32;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace SwiftBinaryProtocol
{
    public abstract class SBPReceiverSenderBase : IDisposable
    {
        #region Constants
        
        public const int SBP_FRAMING_MAX_PAYLOAD_SIZE = 255;

        public const int SBP_CONSOLE_SENDER_ID = 0x42;

        #endregion

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

        private bool _rtsCts = false;

        private IPAddress _ipAdress = IPAddress.Any;

        private int _tcpPort = 55555;

        protected Queue<byte> _receivedBytes;
                
        public const byte PREAMBLE = 0x55;

        public const int SEND_TIMEOUT_MS = 5; //Prevents rx overrun in Piksi

        public const int SEND_TIMEOUT_SERIAL_MS = 50; //Prevents rx overrun in Piksi

        #endregion

        #region Events

        public event EventHandler<SBPSendExceptionEventArgs> SendExeceptionEvent;

        public event EventHandler<SBPReadExceptionEventArgs> ReadExceptionEvent;

        #endregion

        #region ctor

        public SBPReceiverSenderBase(string comPort, int baudrate, bool rtsCts)
        {
            _comPort = comPort;
            _baudRate = baudrate;
            _rtsCts = rtsCts;

            _receiveSendThread = new Thread(new ThreadStart(ReceiveSendThreadSerial));
            _receiveSendThread.Start();

            _invokeThread = new Thread(new ThreadStart(InvokeThread));
            _invokeThread.Start();
        }

        public SBPReceiverSenderBase(IPAddress ipAdress, int tcpPort)
        {
            _ipAdress = ipAdress;
            _tcpPort = tcpPort;

            _receiveSendThread = new Thread(new ThreadStart(ReceiveSendThreadTCP));
            _receiveSendThread.Start();

            _invokeThread = new Thread(new ThreadStart(InvokeThread));
            _invokeThread.Start();
        }

        #endregion

        #region Protected Methods

        private void ReceiveSendThreadSerial()
        {
            byte[] buffer = new byte[32];
            bool restart = false;
            bool rtsActive = false;
            byte[] sendBuffer = new byte[0];
            DateTime sendTimeout = DateTime.MinValue;
            IntPtr portHandle = IntPtr.Zero;
            Thread.Sleep(1000);
            _receivedBytes = new Queue<byte>();
            
            while(!_receiveSendThreadStopped)
            {
                try
                {
                    if (portHandle == IntPtr.Zero)
                    {
                        portHandle = Win32Com.CreateFile(_comPort, Win32Com.GENERIC_READ | Win32Com.GENERIC_WRITE, 0, IntPtr.Zero,
                            Win32Com.OPEN_EXISTING, 0, IntPtr.Zero);

                        if (portHandle == (IntPtr)Win32Com.INVALID_HANDLE_VALUE)
                        {
                            if (Marshal.GetLastWin32Error() == Win32Com.ERROR_ACCESS_DENIED)
                                throw new Exception(String.Format("Access denied for port {0}", _comPort));
                            else
                                throw new Exception(String.Format("Failed to open port {0}", _comPort));
                        }

                        COMMTIMEOUTS commTimeouts = new COMMTIMEOUTS();
                        commTimeouts.ReadIntervalTimeout = uint.MaxValue;
                        commTimeouts.ReadTotalTimeoutConstant = 0;
                        commTimeouts.ReadTotalTimeoutMultiplier = 0;
                        commTimeouts.WriteTotalTimeoutConstant = 0;
                        commTimeouts.WriteTotalTimeoutMultiplier = 0;
                        DCB dcb = new DCB();
                        dcb.Init(false, false, false, 0, false, false, false, false, 0);
                        dcb.BaudRate = _baudRate;
                        dcb.ByteSize = 8;
                        dcb.Parity = 0;
                        dcb.StopBits = 0;
                        if (!Win32Com.SetupComm(portHandle, 8192, 4096))
                            throw new Exception(String.Format("Failed to set queue settings for port {0}", _comPort));
                        if (!Win32Com.SetCommState(portHandle, ref dcb))
                            throw new Exception(String.Format("Failed to set comm settings for port {0}", _comPort));
                        if (!Win32Com.SetCommTimeouts(portHandle, ref commTimeouts))
                            throw new Exception(String.Format("Failed to set comm timeouts for port {0}", _comPort));
                        if(_rtsCts)
                            if (!Win32Com.EscapeCommFunction(portHandle, Win32Com.CLRRTS))
                                throw new Exception(String.Format("Failed to reset RTS pin{0}", _comPort));
                    }

                    uint lpdwFlags = 0;
                    if (!Win32Com.GetHandleInformation(portHandle, out lpdwFlags))
                        throw new Exception(String.Format("Port {0} went offline", _comPort));

                    if (_sendMessageQueue.Count > 0 && sendBuffer.Length == 0)
                        lock (_syncobject)
                            sendBuffer = _sendMessageQueue.Dequeue();

                    if(sendBuffer.Length > 0)
                    {
                        if (!rtsActive && _rtsCts)
                        {
                            if (!Win32Com.EscapeCommFunction(portHandle, Win32Com.SETRTS))
                                lock (_syncobject)
                                    _sendExceptionQueue.Enqueue(new SBPSendExceptionEventArgs(new Exception(String.Format("Failed to set RTS pin{0}", _comPort))));

                            sendTimeout = DateTime.Now.Add(TimeSpan.FromMilliseconds(SEND_TIMEOUT_SERIAL_MS));
                            rtsActive = true;
                        }
                        uint lpmodemstat = 0;
                        if(_rtsCts)
                            if (!Win32Com.GetCommModemStatus(portHandle, out lpmodemstat))
                                _sendExceptionQueue.Enqueue(new SBPSendExceptionEventArgs(new Exception(String.Format("Failed to get RTS pin{0}", _comPort))));

                        if ((lpmodemstat & Win32Com.MS_CTS_ON) > 0 || !_rtsCts)
                        {
                            uint bytesWritten = 0;
                            if (!Win32Com.WriteFile(portHandle, sendBuffer, (uint)sendBuffer.Length, out bytesWritten, IntPtr.Zero))
                                lock (_syncobject)
                                    _sendExceptionQueue.Enqueue(new SBPSendExceptionEventArgs(new Exception(String.Format("Failed to write to port {0}", _comPort))));

                            if (DateTime.Now > sendTimeout)
                                lock (_syncobject)
                                    _sendExceptionQueue.Enqueue(new SBPSendExceptionEventArgs(new Exception(String.Format("Failed to write all bytes to port {0}", _comPort))));

                            if (bytesWritten == sendBuffer.Length || DateTime.Now > sendTimeout)
                            {
                                if (_rtsCts)
                                    if (!Win32Com.EscapeCommFunction(portHandle, Win32Com.CLRRTS))
                                        lock (_syncobject)
                                            _sendExceptionQueue.Enqueue(new SBPSendExceptionEventArgs(new Exception(String.Format("Failed to reset RTS pin{0}", _comPort))));
                                rtsActive = false;
                                sendBuffer = new byte[0];
                            }
                        }
                    }
                    
                    uint bytesRead = 0;
                    if (!Win32Com.ReadFile(portHandle, buffer, (uint)buffer.Length, out bytesRead, IntPtr.Zero))
                        throw new Exception(String.Format("Failed to read port {0}", _comPort));

                    if (bytesRead > 0)
                    {
                        byte[] bytes = new byte[bytesRead];
                        Buffer.BlockCopy(buffer, 0, bytes, 0, (int)bytesRead);
                        lock (_syncobject)
                            foreach (byte Byte in bytes)
                                _receivedBytes.Enqueue(Byte);
                    }
                    else if (sendBuffer.Length == 0)
                        Thread.Sleep(1);

                    ProcessReading(restart);

                    restart = false;
                }
                catch(Exception e)
                {
                    _receivedBytes.Clear();
                    Win32Com.CancelIo(portHandle);
                    Win32Com.CloseHandle(portHandle);
                    portHandle = IntPtr.Zero;
                    restart = true;
                    lock (_syncobject)
                        _readExceptionQueue.Enqueue(new SBPReadExceptionEventArgs(e));

                    Thread.Sleep(5000);
                }
            }
            Win32Com.CancelIo(portHandle);
            Win32Com.CloseHandle(portHandle);
        }

        private void ReceiveSendThreadTCP()
        {
            TcpClient tcpClient = null;
            DateTime sendTimeout = DateTime.MinValue;
            bool restart = false;
            _receivedBytes = new Queue<byte>();

            while (!_receiveSendThreadStopped)
            {
                try
                {
                    if (tcpClient == null)
                    {
                        tcpClient = new TcpClient();
                        tcpClient.Connect(_ipAdress, _tcpPort);
                    }
                    
                    if (_sendMessageQueue.Count > 0 && sendTimeout < DateTime.Now)
                    {
                        byte[] messageToSend;
                        lock (_syncobject)
                            messageToSend = _sendMessageQueue.Dequeue();
                        
                        tcpClient.GetStream().Write(messageToSend, 0, messageToSend.Length);

                        sendTimeout = DateTime.Now.AddMilliseconds((double)SEND_TIMEOUT_MS);
                    }
                    byte[] receivedBytes = new byte[tcpClient.Available];
                    int bytesRead = tcpClient.GetStream().Read(receivedBytes, 0, receivedBytes.Length);

                    if (bytesRead > 0)
                    {
                        lock (_syncobject)
                            foreach (byte Byte in receivedBytes)
                                _receivedBytes.Enqueue(Byte);
                    }
                    else
                        Thread.Sleep(1);

                    ProcessReading(restart);

                    restart = false;
                }
                catch (Exception e)
                {
                    if (tcpClient != null)
                    {
                        tcpClient.Close();
                        tcpClient = null;
                    }
                    _receivedBytes.Clear();
                    restart = true;
                    lock (_syncobject)
                        _readExceptionQueue.Enqueue(new SBPReadExceptionEventArgs(e));

                    Thread.Sleep(5000);
                }
            }
            if (tcpClient != null)
            {
                tcpClient.Close();
                tcpClient = null;
            }
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
                    Thread.Sleep(1);

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
