using System;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct AquisitionResult
    {
        private float _snr;
        
        private float _cp;
        
        private float _cf;

        private byte _sid;

        public AquisitionResult(byte[] data)
        {
            _snr = BitConverter.ToSingle(data, 0);
            _cp = BitConverter.ToSingle(data, 4);
            _cf = BitConverter.ToSingle(data, 8);
            _sid = data[12];
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

        public byte SignalIdentifier
        {
            get { return _sid; }
        }
    }
}
