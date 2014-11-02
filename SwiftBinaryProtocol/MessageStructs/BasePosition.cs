using System;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct BasePosition
    {
        private double _lat;

        private double _lon;

        private double _height;

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
    }
}
