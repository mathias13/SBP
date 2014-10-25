using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct IARState
    {
        private uint _numHyps;

        public IARState(byte[] data)
        {
            _numHyps = BitConverter.ToUInt32(data,0);
        }

        public uint NumberOfHypothesis
        {
            get { return _numHyps; }
        }
    }
}
