﻿using System;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct VelocityECEF_Dep
    {
        private uint _tow;

        private int _x;

        private int _y;

        private int _z;

        private ushort _accuracy;

        private byte _n_sats;
        
        public VelocityECEF_Dep(byte[] data)
        {
            _tow = BitConverter.ToUInt32(data, 0);
            _x = BitConverter.ToInt32(data, 4);
            _y = BitConverter.ToInt32(data, 8);
            _z = BitConverter.ToInt32(data, 12);
            _accuracy = BitConverter.ToUInt16(data, 16);
            _n_sats = data[18];
        }

        public uint TimeOfWeek
        {
            get { return _tow; }
        }

        public int VelocityX
        {
            get { return _x; }
        }

        public int VelocityY
        {
            get { return _y; }
        }

        public int VelocityZ
        {
            get { return _z; }
        }

        public ushort VelocityAccuracyEstimate
        {
            get { return _accuracy; }
        }

        public byte NumberOfSattelites
        {
            get { return _n_sats; }
        }        
    }
}
