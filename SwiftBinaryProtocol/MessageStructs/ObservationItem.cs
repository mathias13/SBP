using System;
using System.Collections.Generic;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct ObservationItem: IPayload
    {
        private uint _p;

        private int _li;

        private byte _lf;

        private short _di;

        private byte _df;

        private byte _cn0;

        private ushort _lockTimer;

        private byte _flags;

        private byte _sid;

        private SBP_Enums.SIDCode _sidCode;
        
        private const double P_MULTIPLIER = 1e2;

        private const float CN0_MULTIPLIER = 4f;

        private const double LF_DOPPLER_MULTIPLIER = (double)(1 << 8);

        public ObservationItem(double pseudoRange, double carrierPhase, double doppler, float carrierToNoiseDensity, ushort lockTimer, bool validPseudorange, bool validCarrierphase, bool halfCyclePhaseAmbiguityResolved, bool validDopplerMeasurement, byte sid, SBP_Enums.SIDCode sidCode)
        {
            _p = (uint)(pseudoRange * P_MULTIPLIER);
            double carrierWhole = Math.Floor(carrierPhase);
            double carrierFraction = (carrierPhase - carrierWhole);
            double dopplerWhole = Math.Floor(carrierPhase);
            double dopplerFraction = (carrierPhase - carrierWhole);
            _li = (int)carrierWhole;
            _lf = (byte)(carrierFraction * LF_DOPPLER_MULTIPLIER);
            _di = (short)dopplerWhole;
            _df = (byte)(dopplerFraction * LF_DOPPLER_MULTIPLIER);
            _cn0 = (byte)(carrierToNoiseDensity * CN0_MULTIPLIER);
            _lockTimer = lockTimer;
            _flags = Convert.ToByte(validPseudorange);
            _flags = (byte)(_flags | (Convert.ToByte(validCarrierphase) << 1));
            _flags = (byte)(_flags | (Convert.ToByte(halfCyclePhaseAmbiguityResolved) << 2));
            _flags = (byte)(_flags | (Convert.ToByte(validDopplerMeasurement) << 3));
            _sid = sid;
            _sidCode = sidCode;
        }

        public ObservationItem(byte[] data)
        {
            _p = BitConverter.ToUInt32(data, 0);
            _li = BitConverter.ToInt32(data, 4);
            _lf = data[8];
            _di = BitConverter.ToInt16(data, 9);
            _df = data[11];
            _cn0 = data[12];
            _lockTimer = data[13];
            _flags = data[14];
            _sid = data[15];
            _sidCode = (SBP_Enums.SIDCode)data[16];
        }

        public byte[] Data
        {
            get
            {
                List<byte> bytes = new List<byte>();
                bytes.AddRange(BitConverter.GetBytes(_p));
                bytes.AddRange(BitConverter.GetBytes(_li));
                bytes.Add(_lf);
                bytes.AddRange(BitConverter.GetBytes(_di));
                bytes.Add(_df);
                bytes.Add(_cn0);
                bytes.AddRange(BitConverter.GetBytes(_lockTimer));
                bytes.Add(_flags);
                bytes.Add(_sid);
                bytes.Add((byte)_sidCode);
                return bytes.ToArray();
            }
        }

        public double P
        {
            get { return (double)_p / P_MULTIPLIER; }
        }

        public double L
        {
            get { return (double)_li + ((double)_lf / LF_DOPPLER_MULTIPLIER); }
        }

        public double Doppler
        {
            get { return (double)_di + ((double)_df / LF_DOPPLER_MULTIPLIER); }
        }

        public float CN0
        {
            get { return (float)_cn0 / CN0_MULTIPLIER; }
        }

        public ushort LockTimer
        {
            get { return _lockTimer; }
        }

        public bool ValidPseudorange
        {
            get { return ((_flags & 0x01) > 0); }
        }

        public bool ValidCarrierphase
        {
            get { return ((_flags & 0x02) > 0); }
        }

        public bool HalfCyclePhaseAmbiguityResolved
        {
            get { return ((_flags & 0x04) > 0); }
        }

        public bool ValidDopplerMeasurement
        {
            get { return ((_flags & 0x08) > 0); }
        }

        public byte SID
        {
            get { return _sid; }
        }

        public SBP_Enums.SIDCode SIDCode
        {
            get { return _sidCode; }
        }
    }
}
