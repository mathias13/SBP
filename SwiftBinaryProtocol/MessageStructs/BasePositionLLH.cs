using System;
using System.Collections.Generic;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct BasePositionLLH: IPayload
    {
        private double _lat;

        private double _lon;

        private double _height;

        public BasePositionLLH(double lat, double lon, double height)
        {
            _lat = lat;
            _lon = lon;
            _height = height;
        }

        public BasePositionLLH(byte[] data)
        {
            _lat = BitConverter.ToDouble(data, 0);
            _lon = BitConverter.ToDouble(data, 8);
            _height = BitConverter.ToDouble(data, 16);
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
