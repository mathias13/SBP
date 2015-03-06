using System;
using System.Text;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct UARTPort
    {
        private float _txThroughput;

        private float _rxThroughput;

        private ushort _crcErrorCount;

        private ushort _ioErrorCount;

        private byte _txBufferLevel;

        private byte _rxBufferLevel;

        public UARTPort(byte[] data)
        {
            _txThroughput = BitConverter.ToSingle(data, 0);
            _rxThroughput = BitConverter.ToSingle(data, 4);
            _crcErrorCount = BitConverter.ToUInt16(data, 8);
            _ioErrorCount = BitConverter.ToUInt16(data, 10);
            _txBufferLevel = data[12];
            _rxBufferLevel = data[13];
        }

        public float TxThroughput
        {
            get { return _txThroughput; }
        }

        public float RxThroughput
        {
            get { return _rxThroughput; }
        }

        public ushort CRCErrorCount
        {
            get { return _crcErrorCount; }
        }

        public ushort IOErrorCount
        {
            get { return _ioErrorCount; }
        }

        public byte TxBufferLevel
        {
            get { return _txBufferLevel; }
        }

        public byte RxBufferLevel
        {
            get { return _rxBufferLevel; }
        }
    }

    public struct UARTState
    {
        private int _avg;

        private int _min;

        private int _max;

        private int _current;

        private UARTPort[] _uartPort;

        public UARTState(byte[] data)
        {
            _uartPort = new UARTPort[3];
            byte[] portData = new byte[14];
            Array.Copy(data, 0, portData, 0, 14);
            _uartPort[0] = new UARTPort(portData);
            Array.Copy(data, 14, portData, 0, 14);
            _uartPort[1] = new UARTPort(portData);
            Array.Copy(data, 28, portData, 0, 14);
            _uartPort[2] = new UARTPort(portData);

            _avg = BitConverter.ToInt32(data, 42);
            _min = BitConverter.ToInt32(data, 46);
            _max = BitConverter.ToInt32(data, 50);
            _current = BitConverter.ToInt32(data, 54);
        }

        public int Average
        {
            get { return _avg; }
        }

        public int Min
        {
            get { return _min; }
        }

        public int Max
        {
            get { return _max; }
        }

        public int Current
        {
            get { return _current; }
        }

        public UARTPort UARTA
        {
            get { return _uartPort[0]; }
        }

        public UARTPort UARTB
        {
            get { return _uartPort[1]; }
        }

        public UARTPort FTDI
        {
            get { return _uartPort[2]; }
        }
    }
}
