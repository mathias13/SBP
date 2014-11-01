using System;
using System.Runtime.InteropServices;

namespace SwiftBinaryProtocol.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct COMMTIMEOUTS
    {
        internal UInt32 ReadIntervalTimeout;
        internal UInt32 ReadTotalTimeoutMultiplier;
        internal UInt32 ReadTotalTimeoutConstant;
        internal UInt32 WriteTotalTimeoutMultiplier;
        internal UInt32 WriteTotalTimeoutConstant;
    }
}