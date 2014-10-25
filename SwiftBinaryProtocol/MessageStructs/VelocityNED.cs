using System;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct VelocityNED
    {
        private uint _tow;

        private int _n;

        private int _e;

        private int _d;

        private ushort _h_accuracy;

        private ushort _v_accuracy;

        private byte _n_sats;
        
        public VelocityNED(byte[] data)
        {
            _tow = BitConverter.ToUInt32(data, 0);
            _n = BitConverter.ToInt32(data, 4);
            _e = BitConverter.ToInt32(data, 8);
            _d = BitConverter.ToInt32(data, 12);
            _h_accuracy = BitConverter.ToUInt16(data, 16);
            _v_accuracy = BitConverter.ToUInt16(data, 18);
            _n_sats = data[20];
        }

        public uint TimeOfWeek
        {
            get { return _tow; }
        }

        public int VelocityNorth
        {
            get { return _n; }
        }

        public int VelocityEast
        {
            get { return _e; }
        }

        public int VelocityDown
        {
            get { return _d; }
        }

        public ushort HorizontalVelocityAccuracyEstimate
        {
            get { return _h_accuracy; }
        }

        public ushort VerticalVelocityAccuracyEstimate
        {
            get { return _v_accuracy; }
        }

        public byte NumberOfSattelites
        {
            get { return _n_sats; }
        }
    }
}
