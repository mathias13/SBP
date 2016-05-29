using System;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct PositionLLH
    {
        private uint _tow;

        private double _lat;

        private double _lon;

        private double _height;

        private ushort _h_accuracy;

        private ushort _v_accuracy;

        private byte _n_sats;

        private SBP_Enums.FixMode _fixMode;

        private bool _heightAboveMeanSeaLevel;

        private bool _raimAvailable;

        private bool _raimRepair;

        public PositionLLH(byte[] data)
        {
            _tow = BitConverter.ToUInt32(data, 0);
            _lat = BitConverter.ToDouble(data, 4);
            _lon = BitConverter.ToDouble(data, 12);
            _height = BitConverter.ToDouble(data, 20);
            _h_accuracy = BitConverter.ToUInt16(data, 28);
            _v_accuracy = BitConverter.ToUInt16(data, 30);
            _n_sats = data[32];            
            _fixMode = (SBP_Enums.FixMode)(data[33] & 0x3);
            _heightAboveMeanSeaLevel = (data[31] & 0x4) > 0;
            _raimAvailable = (data[31] & 0x8) > 0;
            _raimRepair = (data[31] & 0x10) > 0;
        }

        public uint TimeOfWeek
        {
            get { return _tow; }
        }

        public double PosLatitude
        {
            get { return _lat; }
        }

        public double PosLongitude
        {
            get { return _lon; }
        }

        public double PosHeight
        {
            get { return _height; }
        }

        public ushort HorizontalAccuracyEstimate
        {
            get { return _h_accuracy; }
        }

        public ushort VerticalAccuracyEstimate
        {
            get { return _v_accuracy; }
        }

        public byte NumberOfSattelites
        {
            get { return _n_sats; }
        }

        public SBP_Enums.FixMode FixMode
        {
            get { return _fixMode; }
        }

        public bool HeightAboveMeanSeaLevel
        {
            get { return _heightAboveMeanSeaLevel; }
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
