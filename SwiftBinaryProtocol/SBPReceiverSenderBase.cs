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
            byte[] buffer = new byte[32];
            bool restart = false;
            bool rtsActive = false;
            byte[] sendMessageBytes = new byte[0];
            DateTime sendTimeout = DateTime.MinValue;
            IntPtr portHandle = IntPtr.Zero;
            Thread.Sleep(1000);
            _receivedBytes.Clear();
            Task<int> sendTask = null;
            Task<int> readTask = null;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

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
                            Win32Com.OPEN_EXISTING, 0, IntPtr.Zero);

                        if (portHandle == (IntPtr)Win32Com.INVALID_HANDLE_VALUE)
                        {
                            if (Marshal.GetLastWin32Error() == Win32Com.ERROR_ACCESS_DENIED)
                                throw new Exception(String.Format("Access denied for port {0}", _comPort));
                            else
                                throw new Exception(String.Format("Failed to open port {0}", _comPort));
                        }

                        COMMTIMEOUTS commTimeouts = new COMMTIMEOUTS
                        {
                            ReadIntervalTimeout = uint.MaxValue,
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

                        readTask = new Task<int>(() =>
                        {
                            if (!Win32Com.ReadFile(portHandle, buffer, (uint)buffer.Length, out uint bytesRead, IntPtr.Zero))
                                return -1;
                            else
                                return (int)bytesRead;
                        });
                        stopwatch.Restart();
                        readTask.Start();
                    }

                    if (!Win32Com.GetHandleInformation(portHandle, out uint lpdwFlags))
                        throw new Exception(String.Format("Port {0} went offline", _comPort));

                    if (_sendMessageQueue.Count > 0 && sendMessageBytes.Length == 0)
                        lock (_syncobject)
                            sendMessageBytes = _sendMessageQueue.Dequeue();

                    if(sendMessageBytes.Length > 0)
                    {
                        if (!rtsActive)
                        {
                            if(_rtsCts)
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
                            if (sendTask == null)
                            {
                                sendTask = new Task<int>(() => {
                                    if (!Win32Com.WriteFile(portHandle, sendMessageBytes, (uint)sendMessageBytes.Length, out uint bytesWritten, IntPtr.Zero))
                                        return -1;
                                    else
                                        return (int)bytesWritten;
                                });
                                sendTask.Start();
                            }
                        
                            if(sendTask.IsCompleted)
                            {
                                if(sendTask.Result < 0)
                                    lock (_syncobject)
                                        _sendExceptionQueue.Enqueue(new SBPSendExceptionEventArgs(new Exception(String.Format("Failed to write to port {0}", _comPort))));

                                if (sendTask.Result == sendMessageBytes.Length || DateTime.Now > sendTimeout)
                                {
                                    if (_rtsCts)
                                        if (!Win32Com.EscapeCommFunction(portHandle, Win32Com.CLRRTS))
                                            lock (_syncobject)
                                                _sendExceptionQueue.Enqueue(new SBPSendExceptionEventArgs(new Exception(String.Format("Failed to reset RTS pin{0}", _comPort))));
                                    rtsActive = false;
                                    sendTask = null;
                                    sendMessageBytes = new byte[0];
                                }
                            }
                            
                            if (DateTime.Now > sendTimeout)
                                lock (_syncobject)
                                    _sendExceptionQueue.Enqueue(new SBPSendExceptionEventArgs(new Exception(String.Format("Failed to write all bytes to port {0}", _comPort))));

                        }
                    }

                    if (readTask.IsCompleted)
                    {
                        if(stopwatch.ElapsedMilliseconds > 50)
                            lock (_syncobject)
                                _readExceptionQueue.Enqueue(new SBPReadExceptionEventArgs(new Exception("Readtask takes more than 100ms: bytes read" + readTask.Result.ToString())));

                        if (readTask.Result < 0)
                            throw new Exception(String.Format("Failed to read port {0}", _comPort));
                        else
                        {
                            if (readTask.Result > 0)
                                for (int i = 0; i < readTask.Result; i++)
                                    _receivedBytes.Add(buffer[i]);
                            else
                                Thread.Sleep(1);
                        }
                        readTask = new Task<int>(() =>
                        {
                            if (!Win32Com.ReadFile(portHandle, buffer, (uint)buffer.Length, out uint bytesRead, IntPtr.Zero))
                                return -1;
                            else
                                return (int)bytesRead;
                        });
                        readTask.Start();
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
            if (sendTask != null)
                if (!sendTask.IsCompleted)
                    sendTask.Wait();

            if (readTask != null)
                if (!readTask.IsCompleted)
                    readTask.Wait();

            Win32Com.CancelIo(portHandle);
            Win32Com.CloseHandle(portHandle);
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
            var stopwatch = new Stopwatch();
            stopwatch.Start();
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

                if (stopwatch.ElapsedMilliseconds > 100)
                    lock (_syncobject)
                        _readExceptionQueue.Enqueue(new SBPReadExceptionEventArgs(new Exception("InvokeThread takes more than 100ms")));

                stopwatch.Restart();
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
