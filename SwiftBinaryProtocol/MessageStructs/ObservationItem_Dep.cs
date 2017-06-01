using System;
using System.Collections.Generic;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct ObservationItem_Dep: IPayload
    {
        private uint _p;

        private int _li;

        private byte _lf;

        private byte _cn0;

        private ushort _lockCounter;

        private uint _sid;

        private byte _sidCode;
        
        private const double P_MULTIPLIER = 1e2;

        private const float CN0_MULTIPLIER = 4f;

        private const double LF_MULTIPLIER = (double)(1 << 8);

        public ObservationItem_Dep(double pseudoRange, double carrierPhase, float carrierToNoiseDensity, ushort lockCounter, uint sid, byte sidCode)
        {
            _p = (uint)(pseudoRange * P_MULTIPLIER);
            double carrierWhole = Math.Floor(carrierPhase);
            double carrierFraction = (carrierPhase - carrierWhole);
            _li = (int)carrierWhole;
            _lf = (byte)(carrierFraction * LF_MULTIPLIER);
            _cn0 = (byte)(carrierToNoiseDensity * CN0_MULTIPLIER);
            _lockCounter = lockCounter;
            _sid = sid;
            _sidCode = sidCode;
        }

        public ObservationItem_Dep(byte[] data)
        {
            _p = BitConverter.ToUInt32(data, 0);
            _li = BitConverter.ToInt32(data, 4);
            _lf = data[8];
            _cn0 = data[9];
            _lockCounter = data[10];
            _sid = BitConverter.ToUInt32(data, 12);
            _sidCode = data[14];
        }

        public byte[] Data
        {
            get
            {
                List<byte> bytes = new List<byte>();
                bytes.AddRange(BitConverter.GetBytes(_p));
                bytes.AddRange(BitConverter.GetBytes(_li));
                bytes.Add(_lf);
                bytes.Add(_cn0);
                bytes.AddRange(BitConverter.GetBytes(_sid));
                return bytes.ToArray();
            }
        }

        public double P
        {
            get { return (double)_p / P_MULTIPLIER; }
        }

        public double L
        {
            get { return (double)_li + ((double)_lf / LF_MULTIPLIER); }
        }

        public float CN0
        {
            get { return (float)_cn0 / CN0_MULTIPLIER; }
        }

        public ushort LockCounter
        {
            get { return _lockCounter; }
        }

        public uint SID
        {
            get { return _sid; }
        }

        public byte SIDCode
        {
            get { return _sidCode; }
        }
    }
}
