using System;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct Startup
    {
        private uint _reserved;

        public Startup(byte[] data)
        {
            _reserved = BitConverter.ToUInt32(data, 0);
        }
    }
}
