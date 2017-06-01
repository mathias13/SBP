using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct Observation_Dep : IPayload
    {
        private uint _tow;

        private ushort _wn;

        private byte _total;

        private byte _count;

        private ObservationItem_Dep[] _observationItem;

        private const double TOW_MULTIPLIER = 1000.0;

        private const int HEADER_SEQ_SHIFT = 4;

        private const int HEADER_SEQ_MASK = ((1 << 4) - 1);

        public const int MAX_OBSERVATIONS = HEADER_SEQ_MASK;

        public Observation_Dep(double tow, ushort wn, byte total, byte count, ObservationItem_Dep[] observation)
        {
            _tow = (uint)(tow * TOW_MULTIPLIER);
            _wn = wn;
            _total = total;
            _count = count;
            _observationItem = observation;
        }

        public Observation_Dep(byte[] data)
        {
            _tow = BitConverter.ToUInt32(data, 0);
            _wn = BitConverter.ToUInt16(data, 4);
            byte sequence = data[6];
            _total = (byte)((int)sequence >> HEADER_SEQ_SHIFT);
            _count = (byte)((int)sequence & HEADER_SEQ_MASK);

            List<ObservationItem_Dep> observations = new List<ObservationItem_Dep>();
            for(int i = 7; i < data.Length; i += 16)
            {
                byte[] observationBytes = new byte[16];
                Array.Copy(data, i, observationBytes, 0, 16);
                observations.Add(new ObservationItem_Dep(observationBytes));
            }
            _observationItem = observations.ToArray();
        }

        public byte[] Data
        {
            get
            {
                List<byte> bytes = new List<byte>();
                bytes.AddRange(BitConverter.GetBytes(_tow));
                bytes.AddRange(BitConverter.GetBytes(_wn));
                byte sequence = (byte)((int)_total << HEADER_SEQ_SHIFT | (int)_count & HEADER_SEQ_MASK);
                bytes.Add(sequence);
                foreach (ObservationItem_Dep observation in _observationItem)
                    bytes.AddRange(observation.Data);

                return bytes.ToArray();
            }
        }

        public double TimeOfWeek
        {
            get { return (double)_tow / TOW_MULTIPLIER; }
        }

        public ushort WeekNumber
        {
            get { return _wn; }
        }
    
        public byte Total
        {
            get { return _total; }
        }

        public byte Count
        {
            get { return _count; }
        }

        public ObservationItem_Dep[] Observations
        {
            get { return _observationItem; }
        }
        
        public DateTime GPSDateTime
        {
            get
            {
                DateTime datum = new DateTime(1980, 1, 6, 0, 0, 0);
                datum = datum.AddDays((double)_wn * 7);
                datum = datum.AddSeconds(TimeOfWeek);
                return datum;
            }
        }    
    }
}
