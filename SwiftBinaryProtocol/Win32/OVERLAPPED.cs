using System;
using System.Runtime.InteropServices;

namespace SwiftBinaryProtocol.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct OVERLAPPED
    {
        internal UIntPtr Internal;
        internal UIntPtr InternalHigh;
        internal UInt32 Offset;
        internal UInt32 OffsetHigh;
        internal IntPtr hEvent;
    }
}