using System;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct DilutionOfPrecision
    {
        private uint _tow;

        private ushort _gdop;

        private ushort _pdop;

        private ushort _tdop;

        private ushort _hdop;

        private ushort _vdop;

        public DilutionOfPrecision(byte[] data)
        {
            _tow = BitConverter.ToUInt32(data, 0);
            _gdop = BitConverter.ToUInt16(data, 4);
            _pdop = BitConverter.ToUInt16(data, 6);
            _tdop = BitConverter.ToUInt16(data, 8);
            _hdop = BitConverter.ToUInt16(data, 10);
            _vdop = BitConverter.ToUInt16(data, 12);
        }

        public uint TimeOfWeek
        {
            get { return _tow; }
        }

        public ushort GeometricDOP
        {
            get { return _gdop; }
        }

        public ushort PositionDOP
        {
            get { return _pdop; }
        }

        public ushort TimeDOP
        {
            get { return _tdop; }
        }

        public ushort HorizontalDOP
        {
            get { return _hdop; }
        }

        public ushort VerticalDOP
        {
            get { return _vdop; }
        }
    }
}
