using System;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct UTCTime
    {
        private SBP_Enums.TimeSource _timeSource;

        private uint _tow;

        private ushort _year;

        private byte _month;

        private byte _day;

        private byte _hour;

        private byte _minute;

        private byte _second;

        private int _ns;
        
        public UTCTime(byte[] data)
        {
            _timeSource = (SBP_Enums.TimeSource)(data[0] & 0x7);
            _tow = BitConverter.ToUInt32(data, 1);
            _year = BitConverter.ToUInt16(data, 5);
            _month = data[7];
            _day = data[8];
            _hour = data[9];
            _minute = data[10];
            _second = data[11];
            _ns = BitConverter.ToInt32(data, 12);
        }

        public uint TimeOfWeek
        {
            get { return _tow; }
        }

        public ushort Year
        {
            get { return _year; }
        }

        public byte Month
        {
            get { return _month; }
        }

        public byte Day
        {
            get { return _day; }
        }

        public byte Hour
        {
            get { return _hour; }
        }

        public byte Minute
        {
            get { return _minute; }
        }

        public byte Second
        {
            get { return _second; }
        }

        public int NanoSecond
        {
            get { return _ns; }
        }

        public SBP_Enums.TimeSource TimeSource
        {
            get { return _timeSource; }
        }

        public DateTime UTCDateTime
        {
            get
            {
                DateTime datum = new DateTime((int)_year, (int)_month, (int)_day, (int)_hour, (int)_minute, (int)_second);
                datum.AddMilliseconds((double)_ns / 1000.0);
                return datum;
            }
        }
    }
}
