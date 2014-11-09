using SwiftBinaryProtocol.Eventarguments;
using System;
using System.Collections.Generic;
using SwiftBinaryProtocol.Win32;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text;

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
            byte[] buffer = new byte[256];
            bool restart = false;
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
                        dcb.Init(false, true, true, 2, true, false, false, false, 2);
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


                    }

                    uint lpdwFlags = 0;
                    if (!Win32Com.GetHandleInformation(portHandle, out lpdwFlags))
                        throw new Exception(String.Format("Port {0} went offline", _comPort));

                    if (_sendMessageQueue.Count > 0)
                    {
                        byte[] messageToSend;
                        lock (_syncobject)
                            messageToSend = _sendMessageQueue.Dequeue();
                        
                        uint bytesWritten = 0;
                        if (!Win32Com.WriteFile(portHandle, messageToSend, (uint)messageToSend.Length, out bytesWritten, IntPtr.Zero))
                        {
                            if (Marshal.GetLastWin32Error() != Win32Com.ERROR_IO_PENDING)
                                lock (_syncobject)
                                    _sendExceptionQueue.Enqueue(new SBPSendExceptionEventArgs(new Exception(String.Format("Failed to write to port {0}", _comPort))));
                            break;
                        }
                        else
                        {
                            byte[] temp = new byte[messageToSend.Length - bytesWritten];
                            Buffer.BlockCopy(messageToSend, (int)bytesWritten, temp, 0, temp.Length);
                            messageToSend = temp;
                        }
                    }
                    uint bytesRead = 0;
                    if (!Win32Com.ReadFile(portHandle, buffer, (uint)buffer.Length, out bytesRead, IntPtr.Zero))
                    {
                        if (Marshal.GetLastWin32Error() == Win32Com.ERROR_IO_PENDING)
                            Win32Com.CancelIo(portHandle);
                        else
                            throw new Exception(String.Format("Failed to read port {0}", _comPort));
                    }
                    if (bytesRead > 0)
                    {
                        byte[] bytes = new byte[bytesRead];
                        Buffer.BlockCopy(buffer, 0, bytes, 0, (int)bytesRead);
                        lock (_syncobject)
                            foreach (byte Byte in bytes)
                                _receivedBytes.Enqueue(Byte);
                    }
                    else
                        Thread.Sleep(1);

                    Thread.Sleep(0);
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
