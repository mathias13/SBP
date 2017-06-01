using System;
using System.Collections.Generic;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct TrackingChannel
    {
        private SBP_Enums.TrackingState _trackingState;

        private uint _sid;

        private SBP_Enums.SIDCode _sidCode;

        private float _cn0;

        public TrackingChannel(SBP_Enums.TrackingState trackingState, uint sid, SBP_Enums.SIDCode sidCode, float cn0)
        {
            _trackingState = trackingState;
            _sid = sid;
            _sidCode = sidCode;
            _cn0 = cn0;
        }

        public SBP_Enums.TrackingState TrackingState
        {
            get { return _trackingState; }
        }

        public uint SID
        {
            get { return _sid; }
        }

        public SBP_Enums.SIDCode SIDCode
        {
            get { return _sidCode; }
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
            for(int i = 0; i < data.Length; i = i + 9)
            {
                SBP_Enums.TrackingState trackingState = data[i] == 1 ? SBP_Enums.TrackingState.ENABLED : SBP_Enums.TrackingState.DISABLED;
                trackingChannels.Add(new TrackingChannel(trackingState, BitConverter.ToUInt16(data, i + 1), (SBP_Enums.SIDCode)data[i + 3], BitConverter.ToSingle(data, i + 5)));
            }
            _trackingChannels = trackingChannels.ToArray();
        }

        public TrackingChannel[] TrackingChannels
        {
            get { return _trackingChannels; }
        }
    }
}

