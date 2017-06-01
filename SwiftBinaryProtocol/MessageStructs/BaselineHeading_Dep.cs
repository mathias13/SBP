using System;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct BaselineHeading_Dep
    {
        private uint _tow;

        private double _heading;

        private byte _n_sats;

        private SBP_Enums.FixMode_Dep _fixMode;

        private bool _raimAvailable;

        private bool _raimRepair;

        public BaselineHeading_Dep(byte[] data)
        {
            _tow = BitConverter.ToUInt32(data, 0);
            _heading = (double)BitConverter.ToUInt32(data, 4) / 1000.0;
            _n_sats = data[8];
            _fixMode = (data[9] & 0x1) > 0 ? SBP_Enums.FixMode_Dep.Fixed_RTK : SBP_Enums.FixMode_Dep.Float_RTK;
            _raimAvailable = (data[9] & 0x4) > 0;
            _raimRepair = (data[9] & 0x8) > 0;
        }

        public uint TimeOfWeek
        {
            get { return _tow; }
        }
        
        public double Heading
        {
            get { return _heading; }
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
