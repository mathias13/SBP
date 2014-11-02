using System;
using System.Collections.Generic;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct BasePosition: IPayload
    {
        private double _lat;

        private double _lon;

        private double _height;

        public BasePosition(double lat, double lon, double height)
        {
            _lat = lat;
            _lon = lon;
            _height = height;
        }

        public BasePosition(byte[] data)
        {
            _lat = BitConverter.ToDouble(data, 0);
            _lon = BitConverter.ToDouble(data, 4);
            _height = BitConverter.ToDouble(data, 8);
        }
        
        public double PosLatitude
        {
            get { return _lat; }
        }

        public double PosLongitude
        {
            get { return _lon; }
        }

        public double PosHeight
        {
            get { return _height; }
        }

        public byte[] Data
        {
            get
            {
                List<byte> bytes = new List<byte>();
                bytes.AddRange(BitConverter.GetBytes(_lat));
                bytes.AddRange(BitConverter.GetBytes(_lon));
                bytes.AddRange(BitConverter.GetBytes(_height));
                return bytes.ToArray();
            }
        }
    }
}
