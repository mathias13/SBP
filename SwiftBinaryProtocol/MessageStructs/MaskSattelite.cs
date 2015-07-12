using System;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct MaskSattelite : IPayload
    {
        private SBP_Enums.SatteliteMask _mask;

        private byte _prn;

        public MaskSattelite(SBP_Enums.SatteliteMask mask, byte prn)
        {
            _mask = mask;
            _prn = prn;
        }

        public MaskSattelite(byte[] data)
        {
            byte mask = data[0];
            SBP_Enums.SatteliteMask maskEnum = SBP_Enums.SatteliteMask.UNKNOWN;
            if (Enum.IsDefined(typeof(SBP_Enums.SatteliteMask), (int)mask))
                maskEnum = (SBP_Enums.SatteliteMask)(int)mask;
            _mask = maskEnum;
            _prn = data[1];
        }

        public byte[] Data
        {
            get
            {
                return new byte[] { Convert.ToByte((int)_mask), _prn };
            }
        }

        public SBP_Enums.SatteliteMask Mask
        {
            get { return _mask; }
        }

        public byte PRN
        {
            get { return _prn; }
        }
    }
}
