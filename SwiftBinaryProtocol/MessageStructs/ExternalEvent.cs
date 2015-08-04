using System;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct ExternalEvent
    {
        private GPSTime _gpsTime;

        private byte _flags;

        private byte _pin;

        public ExternalEvent(byte[] data)
        {
            _gpsTime = new GPSTime(data);
            _flags = data[10];
            _pin = data[11];
        }
        
        public GPSTime GPSTime
        {
            get { return _gpsTime; }
        }

        public byte Flags
        {
            get { return _flags; }
        }

        public byte PinNumber
        {
            get { return _pin; }
    }
    }
}
