using System;
using System.Collections.Generic;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct MaskSattelite : IPayload
    {
        private SBP_Enums.SatteliteMask _mask;

        private uint _sid;

        public MaskSattelite(SBP_Enums.SatteliteMask mask, uint sid)
        {
            _mask = mask;
            _sid = sid;
        }

        public MaskSattelite(byte[] data)
        {
            byte mask = data[0];
            SBP_Enums.SatteliteMask maskEnum = SBP_Enums.SatteliteMask.UNKNOWN;
            if (Enum.IsDefined(typeof(SBP_Enums.SatteliteMask), (int)mask))
                maskEnum = (SBP_Enums.SatteliteMask)(int)mask;
            _mask = maskEnum;
            _sid = BitConverter.ToUInt32(data, 1);
        }

        public byte[] Data
        {
            get
            {
                List<byte> bytes = new List<byte>();
                bytes.Add(Convert.ToByte((int)_mask));
                bytes.AddRange(BitConverter.GetBytes(_sid));
                return bytes.ToArray();
            }
        }

        public SBP_Enums.SatteliteMask Mask
        {
            get { return _mask; }
        }

        public uint SID
        {
            get { return _sid; }
        }
    }
}
