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
        private int _latencyAvg;

        private int _latencyMin;

        private int _latencyMax;

        private int _latencyCurrent;

        private int _periodAvg;

        private int _periodMin;

        private int _periodMax;

        private int _periodCurrent;

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

            _latencyAvg = BitConverter.ToInt32(data, 42);
            _latencyMin = BitConverter.ToInt32(data, 46);
            _latencyMax = BitConverter.ToInt32(data, 50);
            _latencyCurrent = BitConverter.ToInt32(data, 54);
            _periodAvg = BitConverter.ToInt32(data, 58);
            _periodMin = BitConverter.ToInt32(data, 62);
            _periodMax = BitConverter.ToInt32(data, 66);
            _periodCurrent = BitConverter.ToInt32(data, 70);
        }

        public int LatencyAverage
        {
            get { return _latencyAvg; }
        }

        public int LatencyMin
        {
            get { return _latencyMin; }
        }

        public int LatencyMax
        {
            get { return _latencyMax; }
        }

        public int LatencyCurrent
        {
            get { return _latencyCurrent; }
        }

        public int PeriodAverage
        {
            get { return _periodAvg; }
        }

        public int PeriodMin
        {
            get { return _periodMin; }
        }

        public int PeriodMax
        {
            get { return _periodMax; }
        }

        public int PeriodCurrent
        {
            get { return _periodCurrent; }
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
