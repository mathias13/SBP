using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct MAG_Raw : IPayload
    {
        private uint _tow;

        private byte _ns;

        private short _mag_x;

        private short _mag_y;

        private short _mag_z;

        public MAG_Raw(uint tow, byte ns, short mag_x, short mag_y, short mag_z)
        {
            _tow = tow;
            _ns = ns;
            _mag_x = mag_x;
            _mag_y = mag_y;
            _mag_z = mag_z;
        }

        public MAG_Raw(byte[] data)
        {
            _tow = BitConverter.ToUInt32(data, 0);
            _ns = data[4];
            _mag_x = BitConverter.ToInt16(data, 5);
            _mag_y = BitConverter.ToInt16(data, 7);
            _mag_z = BitConverter.ToInt16(data, 9);
        }

        public byte[] Data
        {
            get
            {
                List<byte> bytes = new List<byte>();
                bytes.AddRange(BitConverter.GetBytes(_tow));
                bytes.Add(_ns);
                bytes.AddRange(BitConverter.GetBytes(_mag_x));
                bytes.AddRange(BitConverter.GetBytes(_mag_y));
                bytes.AddRange(BitConverter.GetBytes(_mag_z));
                return bytes.ToArray();
            }
        }
            
        public DateTime GPSDateTime
        {
            get
            {
                var now = DateTime.Now;
                DateTime datum = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
                datum.AddDays((int)now.DayOfWeek * -1);
                datum = datum.AddMilliseconds(_tow);
                datum.AddMilliseconds(_ns / 256.0);
                return datum;
            }
        }    

        public double Mag_X
        {
            get { return _mag_x; }
        }

        public double Mag_Y
        {
            get { return _mag_y; }
        }

        public double Mag_Z
        {
            get { return _mag_z; }
        }
    }
}
