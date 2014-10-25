using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftBinaryProtocol.Eventarguments
{
    public class SBPReadExceptionEventArgs : EventArgs
    {
        private Exception _e;

        public SBPReadExceptionEventArgs(Exception e)
        {
            _e = e;
        }

        public Exception Exception
        {
            get { return _e; }
        }
    }
}
