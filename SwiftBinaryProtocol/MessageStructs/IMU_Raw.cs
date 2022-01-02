using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct IMU_Raw : IPayload
    {
        private uint _tow;

        private byte _ns;

        private short _acc_x;

        private short _acc_y;

        private short _acc_z;

        private short _gyro_x;

        private short _gyro_y;

        private short _gyro_z;

        public IMU_Raw(uint tow, byte ns, short acc_x, short acc_y, short acc_z, short gyro_x, short gyro_y, short gyro_z)
        {
            _tow = tow;
            _ns = ns;
            _acc_x = acc_x;
            _acc_y = acc_y;
            _acc_z = acc_z;
            _gyro_x = gyro_x;
            _gyro_y = gyro_y;
            _gyro_z = gyro_z;
        }

        public IMU_Raw(byte[] data)
        {
            _tow = BitConverter.ToUInt32(data, 0);
            _ns = data[4];
            _acc_x = BitConverter.ToInt16(data, 5);
            _acc_y = BitConverter.ToInt16(data, 7);
            _acc_z = BitConverter.ToInt16(data, 9);
            _gyro_x = BitConverter.ToInt16(data, 11);
            _gyro_y = BitConverter.ToInt16(data, 13);
            _gyro_z = BitConverter.ToInt16(data, 15);
        }

        public byte[] Data
        {
            get
            {
                List<byte> bytes = new List<byte>();
                bytes.AddRange(BitConverter.GetBytes(_tow));
                bytes.Add(_ns);
                bytes.AddRange(BitConverter.GetBytes(_acc_x));
                bytes.AddRange(BitConverter.GetBytes(_acc_y));
                bytes.AddRange(BitConverter.GetBytes(_acc_z));
                bytes.AddRange(BitConverter.GetBytes(_gyro_x));
                bytes.AddRange(BitConverter.GetBytes(_gyro_y));
                bytes.AddRange(BitConverter.GetBytes(_gyro_z));
                return bytes.ToArray();
            }
        }
            
        public DateTime GPSDateTime
        {
            get
            {
                var now = DateTime.Now;
                DateTime datum = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
                datum = datum.AddDays((int)now.DayOfWeek * -1);
                datum = datum.AddMilliseconds(_tow);
                datum.AddMilliseconds((double)_ns / 256);
                return datum;
            }
        }    

        public short Acc_X
        {
            get { return _acc_x; }
        }

        public short Acc_Y
        {
            get { return _acc_y; }
        }

        public short Acc_Z
        {
            get { return _acc_z; }
        }

        public short Gyro_X
        {
            get { return _gyro_x; }
        }

        public short Gyro_Y
        {
            get { return _gyro_y; }
        }

        public short Gyro_Z
        {
            get { return _gyro_z; }
        }
    }
}
