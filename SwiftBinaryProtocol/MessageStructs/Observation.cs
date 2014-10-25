using System;
using System.Collections.Generic;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct Observation: IPayload
    {
        private uint _p;

        private int _li;

        private byte _lf;

        private byte _snr;

        private ushort _lockCounter;

        private byte _prn;
        
        private const double P_MULTIPLIER = 1e2;

        private const float SNR_MULTIPLIER = 4f;

        private const double LF_MULTIPLIER = (double)(1 << 8);

        public Observation(double pseudoRange, double carrierPhase, float signalToNoiseRatio, ushort lockCounter, byte prn)
        {
            _p = (uint)(pseudoRange * P_MULTIPLIER);
            double carrierWhole = Math.Floor(carrierPhase);
            double carrierFraction = (carrierPhase - carrierWhole);
            _li = (int)carrierWhole;
            _lf = (byte)(carrierFraction * LF_MULTIPLIER);
            _snr = (byte)(signalToNoiseRatio * SNR_MULTIPLIER);
            _lockCounter = lockCounter;
            _prn = prn;
        }

        public Observation(byte[] data)
        {
            _p = BitConverter.ToUInt32(data, 0);
            _li = BitConverter.ToInt32(data, 4);
            _lf = data[8];
            _snr = data[9];
            _lockCounter = data[10];
            _prn = data[12];
        }

        public byte[] Data
        {
            get
            {
                List<byte> bytes = new List<byte>();
                bytes.AddRange(BitConverter.GetBytes(_p));
                bytes.AddRange(BitConverter.GetBytes(_li));
                bytes.Add(_lf);
                bytes.Add(_snr);
                bytes.Add(_prn);
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

        public float SNR
        {
            get { return (float)_snr / SNR_MULTIPLIER; }
        }

        public ushort LockCounter
        {
            get { return _lockCounter; }
        }

        public byte PRN
        {
            get { return _prn; }
        }
    }
}
