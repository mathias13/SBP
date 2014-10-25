using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftBinaryProtocol.Eventarguments
{
    public class SBPSendExceptionEventArgs: EventArgs
    {
        private Exception _e;

        public SBPSendExceptionEventArgs(Exception e)
        {
            _e = e;
        }

        public Exception Exception
        {
            get { return _e; }
        }
    }
}
