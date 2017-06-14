using System;
using System.Collections.Generic;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct BasePositionECEF: IPayload
    {
        private double _x;

        private double _y;

        private double _z;

        public BasePositionECEF(double lat, double lon, double height)
        {
            _x = lat;
            _y = lon;
            _z = height;
        }

        public BasePositionECEF(byte[] data)
        {
            _x = BitConverter.ToDouble(data, 0);
            _y = BitConverter.ToDouble(data, 8);
            _z = BitConverter.ToDouble(data, 16);
        }
        
        public double PosX
        {
            get { return _x; }
        }

        public double PosY
        {
            get { return _y; }
        }

        public double PosZ
        {
            get { return _z; }
        }

        public byte[] Data
        {
            get
            {
                List<byte> bytes = new List<byte>();
                bytes.AddRange(BitConverter.GetBytes(_x));
                bytes.AddRange(BitConverter.GetBytes(_y));
                bytes.AddRange(BitConverter.GetBytes(_z));
                return bytes.ToArray();
            }
        }
    }
}
