using System;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct AquisitionResult
    {
        private float _snr;
        
        private float _cp;
        
        private float _cf;

        private ushort _sid;

        private byte _sidCode;

        public AquisitionResult(byte[] data)
        {
            _snr = BitConverter.ToSingle(data, 0);
            _cp = BitConverter.ToSingle(data, 4);
            _cf = BitConverter.ToSingle(data, 8);
            _sid = BitConverter.ToUInt16(data, 12);
            _sidCode = data[14];
        }

        public float SNR
        {
            get { return _snr; }
        }

        public float CodePhase
        {
            get { return _cp; }
        }

        public float CarrierFrequency
        {
            get { return _cf; }
        }

        public ushort SID
        {
            get { return _sid; }
        }

        public byte SIDCode
        {
            get { return _sidCode; }
        }
    }
}
