using System;
using System.Collections.Generic;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct TrackingChannel
    {
        private SBP_Enums.TrackingState _trackingState;

        private byte _prn;

        private float _cn0;

        public TrackingChannel(SBP_Enums.TrackingState trackingState, byte prn, float cn0)
        {
            _trackingState = trackingState;
            _prn = prn;
            _cn0 = cn0;
        }

        public SBP_Enums.TrackingState TrackingState
        {
            get { return _trackingState; }
        }

        public byte PRN
        {
            get { return _prn; }
        }

        public float cn0
        {
            get { return _cn0; }
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
                trackingChannels.Add(new TrackingChannel(trackingState, data[i + 1], BitConverter.ToSingle(data, i + 2)));
            }
            _trackingChannels = trackingChannels.ToArray();
        }

        public TrackingChannel[] TrackingChannels
        {
            get { return _trackingChannels; }
        }
    }
}

