using System;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct Reset : IPayload
    {
        private bool _restoreDefaultSettings;

        public Reset(bool restoreDefaultSettings)
        {
            _restoreDefaultSettings = restoreDefaultSettings;
        }

        public Reset(byte[] data)
        {
            _restoreDefaultSettings = (data[0] & 0x01) > 0;
        }

        public byte[] Data
        {
            get
            {
                return new byte[] { Convert.ToByte(_restoreDefaultSettings) };
            }
        }
    }
}
