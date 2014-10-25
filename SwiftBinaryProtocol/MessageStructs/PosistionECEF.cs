using System;
using SwiftBinaryProtocol;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct PosistionECEF
    {
        private uint _tow;

        private double _x;

        private double _y;

        private double _z;

        private ushort _accuracy;

        private byte _n_sats;

        private SBP_Enums.FixMode _fixMode;

        public PosistionECEF(byte[] data)
        {
            _tow = BitConverter.ToUInt32(data, 0);
            _x = BitConverter.ToDouble(data, 4);
            _y = BitConverter.ToDouble(data, 12);
            _z = BitConverter.ToDouble(data, 20);
            _accuracy = BitConverter.ToUInt16(data, 28);
            _n_sats = data[30];
            _fixMode = (data[31] & 0x1) > 0 ? SBP_Enums.FixMode.RTK : SBP_Enums.FixMode.SinglePointPosition;
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

        public SBP_Enums.FixMode FixMode
        {
            get { return _fixMode; }
        }

    }
}
