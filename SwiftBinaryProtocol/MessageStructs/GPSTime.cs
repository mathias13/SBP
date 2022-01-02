using System;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct GPSTime
    {
        private ushort _wn;

        private uint _tow;

        private int _ns;

        private SBP_Enums.TimeSource _timeSource;

        public GPSTime(byte[] data)
        {
            _wn = BitConverter.ToUInt16(data, 0);
            _tow = BitConverter.ToUInt32(data, 2);
            _ns = BitConverter.ToInt32(data, 6);
            _timeSource = (SBP_Enums.TimeSource)(data[7] & 0x7);
        }

        public ushort WeekNumber
        {
            get { return _wn; }
        }

        public uint TimeOfWeek
        {
            get { return _tow; }
        }

        public int NanoSecond
        {
            get { return _ns; }
        }

        public SBP_Enums.TimeSource TimeSource
        {
            get { return _timeSource; }
        }

        public DateTime GPSDateTime
        {
            get
            {
                DateTime datum = new DateTime(1980, 1, 6, 0, 0, 0);
                datum = datum.AddDays((double)_wn * 7);
                datum = datum.AddMilliseconds(_tow);
                datum = datum.AddMilliseconds(_ns);
                return datum;
            }
        }
    }
}
