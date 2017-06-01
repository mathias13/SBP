using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct Observation : IPayload
    {
        private uint _tow;

        private int _ns;

        private ushort _wn;

        private byte _total;

        private byte _count;

        private ObservationItem[] _observationItem;
        
        private const int HEADER_SEQ_SHIFT = 4;

        private const int HEADER_SEQ_MASK = ((1 << 4) - 1);

        public const int MAX_OBSERVATIONS = HEADER_SEQ_MASK;

        public Observation(uint tow, int ns, ushort wn, byte total, byte count, ObservationItem[] observation)
        {
            _tow = tow;
            _ns = ns;
            _wn = wn;
            _total = total;
            _count = count;
            _observationItem = observation;
        }

        public Observation(byte[] data)
        {
            _tow = BitConverter.ToUInt32(data, 0);
            _ns = BitConverter.ToInt32(data, 4);
            _wn = BitConverter.ToUInt16(data, 8);
            byte sequence = data[10];
            _total = (byte)((int)sequence >> HEADER_SEQ_SHIFT);
            _count = (byte)((int)sequence & HEADER_SEQ_MASK);

            List<ObservationItem> observations = new List<ObservationItem>();
            for(int i = 11; i < data.Length; i += 17)
            {
                byte[] observationBytes = new byte[17];
                Array.Copy(data, i, observationBytes, 0, 17);
                observations.Add(new ObservationItem(observationBytes));
            }
            _observationItem = observations.ToArray();
        }

        public byte[] Data
        {
            get
            {
                List<byte> bytes = new List<byte>();
                bytes.AddRange(BitConverter.GetBytes(_tow));
                bytes.AddRange(BitConverter.GetBytes(_ns));
                bytes.AddRange(BitConverter.GetBytes(_wn));
                byte sequence = (byte)((int)_total << HEADER_SEQ_SHIFT | (int)_count & HEADER_SEQ_MASK);
                bytes.Add(sequence);
                foreach (ObservationItem observation in _observationItem)
                    bytes.AddRange(observation.Data);

                return bytes.ToArray();
            }
        }

        public uint TimeOfWeek
        {
            get { return _tow; }
        }

        public int NanoSeconds
        {
            get { return _ns; }
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

        public ObservationItem[] Observations
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
                datum.AddMilliseconds((double)_ns / 1000.0);
                return datum;
            }
        }    
    }
}
