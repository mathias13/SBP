using System;
using System.Collections.Generic;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct TrackingChannel
    {
        private SBP_Enums.TrackingState _trackingState;

        private byte _prn;

        public TrackingChannel(SBP_Enums.TrackingState trackingState, byte prn)
        {
            _trackingState = trackingState;
            _prn = prn;
        }

        public SBP_Enums.TrackingState TrackingState
        {
            get { return _trackingState; }
        }

        public byte PRN
        {
            get { return _prn; }
        }
    }

    public struct TrackingState
    {
        private TrackingChannel[] _trackingChannels;

        public TrackingState(byte[] data)
        {
            List<TrackingChannel> trackingChannels = new List<TrackingChannel>();
            for(int i = 0; i < data.Length; i = i + 2)
            {
                SBP_Enums.TrackingState trackingState = data[i] == 1 ? SBP_Enums.TrackingState.ENABLED : SBP_Enums.TrackingState.DISABLED;
                trackingChannels.Add(new TrackingChannel(trackingState, data[i + 1]));
            }
            _trackingChannels = trackingChannels.ToArray();
        }

        public TrackingChannel[] TrackingChannels
        {
            get { return _trackingChannels; }
        }
    }
}

