using System;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct ExternalEvent
    {
        private GPSTime _gpsTime;
        
        private bool _newLevel;

        private bool _timeQualityGood;

        private byte _pin;

        public ExternalEvent(byte[] data)
        {
            _gpsTime = new GPSTime(data);
            _newLevel = (data[10] & 0x1) > 0;
            _timeQualityGood = (data[10] & 0x2) > 0;
            _pin = data[11];
        }
        
        public GPSTime GPSTime
        {
            get { return _gpsTime; }
        }
        
        public bool NewLevel
        {
            get { return _newLevel; }
        }

        public bool TimeQualityGood
        {
            get { return _timeQualityGood; }
        }

        public byte PinNumber
        {
            get { return _pin; }
    }
    }
}
