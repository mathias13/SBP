﻿using System;

namespace SwiftBinaryProtocol
{
    public class SBP_Enums
    {
        public enum MessageTypes
        {
            Unknown = 0x0000,
            PRINT = 0x0010,
            ACQ_RESULT = 0x0014,
            TRACKING_STATE = 0x0013,
            THREAD_STATE =  0x0017,
            UART_STATE = 0x001D,
            IAR_STATE = 0x0019,
            MASK_SATELLITE = 0x001B,
            RESET_FILTERS = 0x0022,
            INIT_BASE = 0x0023,
            BASEPOS_LLH = 0x0044,
            BASEPOS_ECEF = 0x0048,
            OBSERVATION_DEP = 0x0049,
            OBSERVATION = 0x004A,
            EPHEMERIS = 0x0080,
            RESET = 0x00B6,
            GPSTIME_DEP = 0x0100,
            EXT_EVENT = 0x0101,
            GPSTIME = 0x0102,
            UTCTIME = 0x0103,
            POS_ECEF_DEP = 0x0200,
            POS_LLH_DEP = 0x0201,
            BASELINE_ECEF_DEP = 0x0202,
            BASELINE_NED_DEP = 0x0203,
            VEL_ECEF_DEP = 0x0204,
            VEL_NED_DEP = 0x0205,
            DOPS_DEP = 0x0206,
            BASELINE_HEADING_DEP = 0x207,
            DOPS = 0x0208,
            POS_ECEF = 0x0209,
            POS_LLH = 0x020A,
            BASELINE_ECEF = 0x020B,
            BASELINE_NED = 0x020C,
            VEL_ECEF = 0x020D,
            VEL_NED = 0x020E,
            BASELINE_HEADING = 0x20F,
            LOG = 0x0401,
            IMU_RAW = 0x0900,
            MAG_RAW = 0x0902,
            STARTUP = 0xFF00,
            HEARTBEAT = 0xFFFF            
        }

        public enum StartupCause
        {
            PowerOn = 0,
            SoftwareReset = 1,
            WatchdogReset = 2
        }

        public enum StartupType
        {
            ColdStart = 0,
            WarmStart = 1,
            HotStart = 2
        }

        public enum TimeSource
        {
            NONE = 0,
            GNSSSolution = 1
        }

        public enum FixMode
        {
            Invalid = 0,
            SinglePointPosition = 1,
            DifferentialGNSS = 2,
            Float_RTK = 3,
            Fixed_RTK = 4
        }

        public enum FixMode_Dep
        {
            SinglePointPosition = 0,
            Fixed_RTK = 1,
            Float_RTK = 2
        }

        public enum VelocityMode
        {
            Invalid = 0,
            MeasuredDopplerDerived = 1,
            ComputedDopplerDerived = 2
        }

        public enum SIDCode
        {
            GPS_L1CA = 0,
            GPS_L2CM = 1,
            SBAS_L1CA = 2,
            GLO_L1CA = 3,
            GLO_L2CA = 4,
            GPS_L1P = 5,
            GPS_L2P = 6
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
