using System;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct Startup
    {
        private SBP_Enums.StartupCause _startupCause;

        private SBP_Enums.StartupType _startupType;

        private uint _reserved;

        public Startup(byte[] data)
        {
            _startupCause = (SBP_Enums.StartupCause)data[0];
            _startupType = (SBP_Enums.StartupType)data[1];
            _reserved = BitConverter.ToUInt16(data, 0);
        }

        public SBP_Enums.StartupCause StartupCause
        {
            get { return _startupCause; }
        }

        public SBP_Enums.StartupType StartupType
        {
            get { return _startupType; }
        }
    }
}
