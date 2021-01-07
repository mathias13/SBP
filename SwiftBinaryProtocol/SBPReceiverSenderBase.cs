using SwiftBinaryProtocol.Eventarguments;
using System;
using System.Collections.Generic;
using SwiftBinaryProtocol.Win32;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using SwiftBinaryProtocol.MessageStructs;

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

        private readonly int _baudRate = 19200;

        private readonly bool _rtsCts = false;

        private readonly IPAddress _ipAdress = IPAddress.Any;

        private readonly int _tcpPort = 55555;
        
        protected List<byte> _receivedBytes = new List<byte>(512);
        
        public const byte PREAMBLE = 0x55;
        
        public const int SEND_TIMEOUT_SERIAL_MS = 500;

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
            _receiveSendThread.Priority = ThreadPriority.Highest;
            _receiveSendThread.Start();

            _invokeThread = new Thread(new ThreadStart(InvokeThread));
            _invokeThread.Priority = ThreadPriority.Highest;
            _invokeThread.Start();
        }

        public SBPReceiverSenderBase(IPAddress ipAdress, int tcpPort)
        {
            _ipAdress = ipAdress;
            _tcpPort = tcpPort;

            _receiveSendThread = new Thread(new ThreadStart(ReceiveSendThreadTCP));
            _receiveSendThread.Priority = ThreadPriority.Highest;
            _receiveSendThread.Start();

            _invokeThread = new Thread(new ThreadStart(InvokeThread));
            _invokeThread.Priority = ThreadPriority.Highest;
            _invokeThread.Start();
        }

        #endregion

        #region Protected Methods

        private void ReceiveSendThreadSerial()
        {
            var buffer = new byte[64];
            var restart = false;
            var sendMessageBytes = new byte[0];
            int sentBytes = 0;
            var sendTimeout = new Stopwatch();
            var portHandle = IntPtr.Zero;
            Thread.Sleep(1000);
            _receivedBytes.Clear();
            var resetEvent = new AutoResetEvent(false);
            var overlapped = new OVERLAPPED();
            var ptrOverlapped = Marshal.AllocHGlobal(Marshal.SizeOf(overlapped));
            overlapped.Offset = 0; 
            overlapped.OffsetHigh = 0;
            overlapped.hEvent = resetEvent.SafeWaitHandle.DangerousGetHandle();
            Marshal.StructureToPtr(overlapped, ptrOverlapped, true);
            bool waitingRead = false;
            var writeResetEvent = new AutoResetEvent(false);
            var writeOverlapped = new OVERLAPPED();
            var ptrWriteOverlapped = Marshal.AllocHGlobal(Marshal.SizeOf(writeOverlapped));
            writeOverlapped.Offset = 0;
            writeOverlapped.OffsetHigh = 0;
            writeOverlapped.hEvent = writeResetEvent.SafeWaitHandle.DangerousGetHandle(); ;
            Marshal.StructureToPtr(writeOverlapped, ptrWriteOverlapped, true);
            bool waitingWrite = false;

            while (!_receiveSendThreadStopped)
            {
                try
                {
                    if (portHandle == IntPtr.Zero)
                    {
                        int portnumber = Int32.Parse(_comPort.Replace("COM", String.Empty));
                        string comPort = _comPort;
                        if (portnumber > 9)
                            comPort = String.Format("\\\\.\\{0}", _comPort);
                        portHandle = Win32Com.CreateFile(comPort, Win32Com.GENERIC_READ | Win32Com.GENERIC_WRITE, 0, IntPtr.Zero,
                            Win32Com.OPEN_EXISTING, Win32Com.FILE_FLAG_OVERLAPPED, IntPtr.Zero);

                        if (portHandle == (IntPtr)Win32Com.INVALID_HANDLE_VALUE)
                        {
                            if (Marshal.GetLastWin32Error() == Win32Com.ERROR_ACCESS_DENIED)
                                throw new Exception(String.Format("Access denied for port {0}", _comPort));
                            else
                                throw new Exception(String.Format("Failed to open port {0}", _comPort));
                        }

                        COMMTIMEOUTS commTimeouts = new COMMTIMEOUTS
                        {
                            ReadIntervalTimeout = 5,
                            ReadTotalTimeoutConstant = 0,
                            ReadTotalTimeoutMultiplier = 0,
                            WriteTotalTimeoutConstant = 0,
                            WriteTotalTimeoutMultiplier = 0
                        };
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
                        Thread.Sleep(1000);
                    }

                    if (!Win32Com.GetHandleInformation(portHandle, out uint lpdwFlags))
                        throw new Exception(String.Format("Port {0} went offline", _comPort));

                    if (!waitingRead)
                    {
                        waitingRead = true;
                        if (!Win32Com.ReadFile(portHandle, buffer, (uint)buffer.Length, out uint readBytes, ptrOverlapped))
                            if (Marshal.GetLastWin32Error() != Win32Com.ERROR_IO_PENDING)
                                throw new Exception(String.Format("Failed to read port {0}", _comPort));
                    }
                    else
                    {
                        if (resetEvent.WaitOne(10))
                        {
                            waitingRead = false;
                            if(!Win32Com.GetOverlappedResult(portHandle, ptrOverlapped, out uint readBytes, false))
                                throw new Exception(String.Format("Failed to read port {0}", _comPort));

                            for (int i = 0; i < readBytes; i++)
                                _receivedBytes.Add(buffer[i]);
                        }
                    }

                    if (_sendMessageQueue.Count > 0 && sendMessageBytes.Length == 0)
                    {
                        lock (_syncobject)
                            sendMessageBytes = _sendMessageQueue.Dequeue();
                        sentBytes = 0;
                    }

                    if (sendMessageBytes.Length > 0)
                    {
                        if(_rtsCts)
                            if (!Win32Com.EscapeCommFunction(portHandle, Win32Com.SETRTS))
                                lock (_syncobject)
                                    _sendExceptionQueue.Enqueue(new SBPSendExceptionEventArgs(new Exception(String.Format("Failed to set RTS pin{0}", _comPort))));

                        uint lpmodemstat = 0;
                        if(_rtsCts)
                            if (!Win32Com.GetCommModemStatus(portHandle, out lpmodemstat))
                                _sendExceptionQueue.Enqueue(new SBPSendExceptionEventArgs(new Exception(String.Format("Failed to get RTS pin{0}", _comPort))));

                        if ((lpmodemstat & Win32Com.MS_CTS_ON) > 0 || !_rtsCts)
                        {
                            if (!waitingWrite)
                            {
                                waitingWrite = true;
                                sendTimeout.Restart();
                                if (!Win32Com.WriteFile(portHandle, sendMessageBytes, (uint)sendMessageBytes.Length, out uint sent, ptrWriteOverlapped))
                                    if (Marshal.GetLastWin32Error() != Win32Com.ERROR_IO_PENDING)
                                        _sendExceptionQueue.Enqueue(new SBPSendExceptionEventArgs(new Exception(String.Format("Failed to write to port {0}", _comPort))));
                            }
                            else
                            {
                                if (writeResetEvent.WaitOne(0))
                                {
                                    if (!Win32Com.GetOverlappedResult(portHandle, ptrWriteOverlapped, out uint sent, false))
                                        _sendExceptionQueue.Enqueue(new SBPSendExceptionEventArgs(new Exception(String.Format("Failed to write to port {0}", _comPort))));

                                    sentBytes += (int)sent;
                                }
                            }

                            if (sentBytes == sendMessageBytes.Length || sendTimeout.ElapsedMilliseconds > 100)
                            {
                                if (sendTimeout.ElapsedMilliseconds > 100)
                                    lock (_syncobject)
                                        _sendExceptionQueue.Enqueue(new SBPSendExceptionEventArgs(new Exception(String.Format("Failed to write all bytes to port {0}", _comPort))));

                                if (_rtsCts)
                                    if (!Win32Com.EscapeCommFunction(portHandle, Win32Com.CLRRTS))
                                        lock (_syncobject)
                                            _sendExceptionQueue.Enqueue(new SBPSendExceptionEventArgs(new Exception(String.Format("Failed to reset RTS pin{0}", _comPort))));

                                sendMessageBytes = new byte[0];
                                waitingWrite = false;
                            }
                        }
                    }

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
            Marshal.FreeHGlobal(ptrOverlapped);
            Marshal.FreeHGlobal(ptrWriteOverlapped);
        }
        
        private void ReceiveSendThreadTCP()
        {
            TcpClient tcpClient = null;
            bool restart = false;
            _receivedBytes.Clear();

            while (!_receiveSendThreadStopped)
            {
                try
                {
                    if (tcpClient == null)
                    {
                        tcpClient = new TcpClient();
                        tcpClient.Connect(_ipAdress, _tcpPort);
                    }
                    
                    if (_sendMessageQueue.Count > 0)
                    {
                        byte[] messageToSend;
                        lock (_syncobject)
                            messageToSend = _sendMessageQueue.Dequeue();
                        
                        tcpClient.GetStream().Write(messageToSend, 0, messageToSend.Length);                        
                    }
                    byte[] receivedBytes = new byte[tcpClient.Available];
                    int bytesRead = tcpClient.GetStream().Read(receivedBytes, 0, receivedBytes.Length);
                    if (bytesRead > 0)
                        _receivedBytes.AddRange(receivedBytes);
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
            SBPSendExceptionEventArgs sendException = null;
            SBPReadExceptionEventArgs readException = null;
            while (!_invokeThreadStop)
            {
                if (_sendExceptionQueue.Count > 0)
                    lock(_syncobject)
                        sendException = _sendExceptionQueue.Dequeue();

                if (_readExceptionQueue.Count > 0)
                    lock (_syncobject)
                        readException = _readExceptionQueue.Dequeue();

                if (sendException != null)
                {
                    OnSendException(sendException);
                    sendException = null;
                }
                if (readException != null)
                {
                    OnReadException(readException);
                    readException = null;
                }
                if (!InvokeThreadExecute() && readException == null && sendException == null)
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
