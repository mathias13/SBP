﻿using System;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct InitBase : IPayload
    {
        public InitBase(byte[] data)
        {
        }

        public byte[] Data
        {
            get { return new byte[0]; }
        }
    }
}
