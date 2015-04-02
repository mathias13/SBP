using System;

namespace SwiftBinaryProtocol
{
    public class SBP_Enums
    {
        public enum MessageTypes
        {
            Unknown = 0,
            PRINT = 16,
            THREAD_STATE =  23,
            UART_STATE = 24,
            IAR_STATE = 25,
            RESET_FILTERS = 34,
            INIT_BASE = 35,
            OBS = 65,
            BASEPOS = 68,
            OBS_HDR = 69,
            GPSTIME = 256,
            POS_ECEF = 512,
            POS_LLH = 513,
            BASELINE_ECEF = 514,
            BASELINE_NED = 515,
            VEL_ECEF = 516,
            VEL_NED = 517,
            DOPS = 518,   
            STARTUP = 65280,
            HEARTBEAT = 65535            
        }

        public enum FixMode
        {
            SinglePointPosition = 0,
            Float_RTK = 1,
            Fixed_RTK = 2
        }
    }
}
