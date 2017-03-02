using System;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct BaselineECEF
    {
        private uint _tow;

        private int _x;

        private int _y;

        private int _z;

        private ushort _accuracy;

        private byte _n_sats;

        private SBP_Enums.FixMode _fixMode;

        private bool _raimRepair;

        public BaselineECEF(byte[] data)
        {
            _tow = BitConverter.ToUInt32(data, 0);
            _x = BitConverter.ToInt32(data, 4);
            _y = BitConverter.ToInt32(data, 8);
            _z = BitConverter.ToInt32(data, 12);
            _accuracy = BitConverter.ToUInt16(data, 16);
            _n_sats = data[18];
            _fixMode = (SBP_Enums.FixMode)(data[19] & 0x7);
            _raimRepair = (data[19] & 0x80) > 0;
        }

        public uint TimeOfWeek
        {
            get { return _tow; }
        }

        public int BaselineX
        {
            get { return _x; }
        }

        public double BaselineXMeters
        {
            get { return (double)_x / 1000.0; }
        }

        public int BaselineY
        {
            get { return _y; }
        }

        public double BaselineYMeters
        {
            get { return (double)_y / 1000.0; }
        }

        public int BaselineZ
        {
            get { return _z; }
        }

        public double BaselineZMeters
        {
            get { return (double)_z / 1000.0; }
        }

        public ushort AccuracyEstimate
        {
            get { return _accuracy; }
        }

        public byte NumberOfSattelites
        {
            get { return _n_sats; }
        }

        public SBP_Enums.FixMode FixMode
        {
            get { return _fixMode; }
        }
        
        public bool RaimRepair
        {
            get { return _raimRepair; }
        }
    }
}
