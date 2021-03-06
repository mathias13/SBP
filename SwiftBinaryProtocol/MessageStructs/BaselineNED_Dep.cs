﻿using System;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct BaselineNED_Dep
    {
        private uint _tow;

        private int _n;

        private int _e;

        private int _d;

        private ushort _h_accuracy;

        private ushort _v_accuracy;

        private byte _n_sats;

        private SBP_Enums.FixMode_Dep _fixMode;

        private bool _raimAvailable;

        private bool _raimRepair;

        public BaselineNED_Dep(byte[] data)
        {
            _tow = BitConverter.ToUInt32(data, 0);
            _n = BitConverter.ToInt32(data, 4);
            _e = BitConverter.ToInt32(data, 8);
            _d = BitConverter.ToInt32(data, 12);
            _h_accuracy = BitConverter.ToUInt16(data, 16);
            _v_accuracy = BitConverter.ToUInt16(data, 18);
            _n_sats = data[20];
            _fixMode = (data[21] & 0x1) > 0 ? SBP_Enums.FixMode_Dep.Fixed_RTK : SBP_Enums.FixMode_Dep.Float_RTK;
            _raimAvailable = (data[21] & 0x4) > 0;
            _raimRepair = (data[21] & 0x8) > 0;
        }

        public uint TimeOfWeek
        {
            get { return _tow; }
        }

        public int BaselineNorth
        {
            get { return _n; }
        }

        public double BaselineNorthMeters
        {
            get { return (double)_n / 1000; }
        }

        public int BaselineEast
        {
            get { return _e; }
        }

        public double BaselineEastMeters
        {
            get { return (double)_e / 1000; }
        }

        public int BaselineDown
        {
            get { return _d; }
        }

        public double BaselineDownMeters
        {
            get { return (double)_d / 1000; }
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
