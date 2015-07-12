using System;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct Reset : IPayload
    {
        public Reset()
        {
        }

        public Reset(byte[] data)
        {
        }

        public byte[] Data
        {
            get
            {
                return new byte[] {};
            }
        }
    }
}
