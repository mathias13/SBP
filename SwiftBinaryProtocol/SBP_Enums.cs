using System;

namespace SwiftBinaryProtocol
{
    public class SBP_Enums
    {
        public enum MessageTypes
        {
            Unknown = 0x0000,
            PRINT = 0x0010,
            TWEET = 0x0012,
            ACQ_RESULT = 0x0014,
            TRACKING_STATE = 0x0013,
            THREAD_STATE =  0x0017,
            UART_STATE = 0x0018,
            IAR_STATE = 0x0019,
            MASK_SATELLITE = 0x001B,
            RESET_FILTERS = 0x0022,
            INIT_BASE = 0x0023,
            OBS_HDR = 0x0043,
            BASEPOS_LLH = 0x0044,
            EPHEMERIS = 0x0047,
            BASEPOS_ECEF = 0x0048,
            RESET = 0x00B2,
            GPSTIME = 0x0100,
            EXT_EVENT = 0x0101,
            POS_ECEF = 0x0200,
            POS_LLH = 0x0201,
            BASELINE_ECEF = 0x0202,
            BASELINE_NED = 0x0203,
            VEL_ECEF = 0x0204,
            VEL_NED = 0x0205,
            DOPS = 0x0206,
            BASELINE_HEADING = 0x207,
            LOG = 0x0401,
            STARTUP = 0xFF00,
            HEARTBEAT = 0xFFFF            
        }

        public enum FixMode
        {
            SinglePointPosition = 0,
            Fixed_RTK = 1,
            Float_RTK = 2
        }

        public enum SatteliteMask
        {
            MASK_ACQUISITION = 1,
            MASK_TRACKING = 2,
            UNKNOWN = -1
        }

        public enum ResetFilter
        {
            IAR = 1,
            DGNSS = 0,
            UNKNOWN = -1
        }

        public enum TrackingState
        {
            DISABLED = 0,
            ENABLED = 1,
            UNKNOWN = -1
        }
    }
}
