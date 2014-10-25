﻿using System;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct Heartbeat
    {
        private bool _externalAntennaPresent;

        private bool _swiftNapError;

        private bool _ioError;

        private bool _systemError;

        public Heartbeat(byte[] data)
        {
            uint dataUint = BitConverter.ToUInt32(data, 0);
            _externalAntennaPresent = ((dataUint & 0x80000000) > 0);
            _swiftNapError = ((dataUint & 0x4) > 0);
            _ioError = ((dataUint & 0x2) > 0);
            _systemError = ((dataUint & 0x1) > 0);
        }

        public bool ExternalAntennaPresent
        {
            get { return _externalAntennaPresent; }
        }

        public bool SwiftNapError
        {
            get { return _swiftNapError; }
        }

        public bool IOError
        {
            get { return _ioError; }
        }

        public bool SystemError
        {
            get { return _systemError; }
        }
    }
}
