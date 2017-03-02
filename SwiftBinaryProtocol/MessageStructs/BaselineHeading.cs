using System;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct BaselineHeading
    {
        private uint _tow;

        private double _heading;

        private byte _n_sats;

        private SBP_Enums.FixMode _fixMode;
        
        private bool _raimRepair;

        public BaselineHeading(byte[] data)
        {
            _tow = BitConverter.ToUInt32(data, 0);
            _heading = (double)BitConverter.ToUInt32(data, 4) / 1000.0;
            _n_sats = data[8];
            _fixMode = (SBP_Enums.FixMode)(data[9] & 0x7);
            _raimRepair = (data[9] & 0x80) > 0;
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
