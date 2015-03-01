using System;
using System.Text;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct ThreadState
    {
        private string _name;

        private ushort _cpu;

        private uint _stackFree;

        public ThreadState(byte[] data)
        {
            byte[] nameBytes = new byte[20];
            Array.Copy(data, 0, nameBytes, 0, 20);
            _name = Encoding.UTF8.GetString(nameBytes);
            _cpu = BitConverter.ToUInt16(data, 20);
            _stackFree = BitConverter.ToUInt32(data, 22);
        }

        public string Name
        {
            get { return _name; }
        }

        public ushort CPU
        {
            get { return _cpu; }
        }

        public uint StackFree
        {
            get { return _stackFree; }
        }

    }
}
