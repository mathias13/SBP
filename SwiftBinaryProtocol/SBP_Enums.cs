using System;

namespace SwiftBinaryProtocol
{
    public class SBP_Enums
    {
        public enum MessageTypes
        {
            Unknown = 0,
            STARTUP = 65280,
            HEARTBEAT = 65535,
            GPSTIME = 256,
            DOPS = 518,
            POS_ECEF = 512,
            POS_LLH = 513,
            BASELINE_ECEF = 514,
            BASELINE_NED = 515,
            VEL_ECEF = 516,
            VEL_NED = 517,    
            OBS = 65,
            OBS_HDR = 69,
            IAR_STATE = 25,
            PRINT = 16
        }

        public enum FixMode
        {
            SinglePointPosition,
            RTK
        }
    }
}
