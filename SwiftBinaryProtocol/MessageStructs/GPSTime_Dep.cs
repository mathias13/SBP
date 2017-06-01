using System;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct GPSTime_Dep
    {
        private ushort _wn;

        private uint _tow;

        private int _ns;

        public GPSTime_Dep(byte[] data)
        {
            _wn = BitConverter.ToUInt16(data, 0);
            _tow = BitConverter.ToUInt32(data, 2);
            _ns = BitConverter.ToInt32(data, 6);
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

        public DateTime GPSDateTime
        {
            get
            {
                DateTime datum = new DateTime(1980, 1, 6, 0, 0, 0);
                datum = datum.AddDays((double)_wn * 7);
                datum = datum.AddMilliseconds((double)_tow);
                datum = datum.AddMilliseconds((double)_ns / 1000);
                return datum;
            }
        }
    }
}
