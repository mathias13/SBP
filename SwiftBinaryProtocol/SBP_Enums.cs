using System;

namespace SwiftBinaryProtocol
{
    public class SBP_Enums
    {
        public enum MessageTypes
        {
            Unknown = 0x0000,
            PRINT = 0x0010,
            ACQ_RESULT = 0x0015,
            THREAD_STATE =  0x0017,
            UART_STATE = 0x0018,
            IAR_STATE = 0x0019,
            RESET_FILTERS = 0x0022,
            INIT_BASE = 0x0023,
            OBS = 0x0041,
            BASEPOS = 0x0044,
            OBS_HDR = 0x0045,
            GPSTIME = 0x0100,
            POS_ECEF = 0x0200,
            POS_LLH = 0x0201,
            BASELINE_ECEF = 0x0202,
            BASELINE_NED = 0x0203,
            VEL_ECEF = 0x0204,
            VEL_NED = 0x0205,
            DOPS = 0x0206,   
            STARTUP = 0xFF00,
            HEARTBEAT = 0xFFFF            
        }

        public enum FixMode
        {
            SinglePointPosition = 0,
            Fixed_RTK = 1,
            Float_RTK = 2
        }
    }
}
