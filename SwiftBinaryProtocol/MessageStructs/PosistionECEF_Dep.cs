using System;
using SwiftBinaryProtocol;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct PosistionECEF_Dep
    {
        private uint _tow;

        private double _x;

        private double _y;

        private double _z;

        private ushort _accuracy;

        private byte _n_sats;

        private SBP_Enums.FixMode_Dep _fixMode;

        private bool _raimAvailable;

        private bool _raimRepair;

        public PosistionECEF_Dep(byte[] data)
        {
            _tow = BitConverter.ToUInt32(data, 0);
            _x = BitConverter.ToDouble(data, 4);
            _y = BitConverter.ToDouble(data, 12);
            _z = BitConverter.ToDouble(data, 20);
            _accuracy = BitConverter.ToUInt16(data, 28);
            _n_sats = data[30];
            _fixMode = (SBP_Enums.FixMode_Dep)(data[31] & 0x3);
            _raimAvailable = (data[31] & 0x4) > 0;
            _raimRepair = (data[31] & 0x8) > 0;
        }

        public uint TimeOfWeek
        {
            get { return _tow; }
        }

        public double PosX
        {
            get { return _x; }
        }

        public double PosY
        {
            get { return _y; }
        }

        public double PosZ
        {
            get { return _z; }
        }

        public ushort AccuracyEstimate
        {
            get { return _accuracy; }
        }

        public byte NumberOfSattelites
        {
            get { return _n_sats; }
        }

        public SBP_Enums.FixMode_Dep FixMode
        {
            get { return _fixMode; }
        }

        public bool RaimAvailable
        {
            get { return _raimAvailable; }
        }
        
        public bool RaimRepair
        {
            get { return _raimRepair; }
        }
    }
}
