using System;
using System.Runtime.InteropServices;

namespace SwiftBinaryProtocol.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct COMMTIMEOUTS
    {
        internal Int32 ReadIntervalTimeout;
        internal Int32 ReadTotalTimeoutMultiplier;
        internal Int32 ReadTotalTimeoutConstant;
        internal Int32 WriteTotalTimeoutMultiplier;
        internal Int32 WriteTotalTimeoutConstant;
    }
}