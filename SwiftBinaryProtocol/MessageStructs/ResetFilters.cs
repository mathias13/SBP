using System;
using System.Collections.Generic;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct ResetFilters: IPayload
    {
        public enum FilterEnum
        {
            IAR = 1,
            DGNSS = 0,
            UNKNOWN = -1
        }

        private FilterEnum _filter;

        public ResetFilters(FilterEnum filter)
        {
            _filter = filter;
        }

        public ResetFilters(byte[] data)
        {
            int filter = Convert.ToInt32(data[0]);
            FilterEnum filterTypeEnum = ResetFilters.FilterEnum.UNKNOWN;
            if (Enum.IsDefined(typeof(FilterEnum), (int)filter))
                filterTypeEnum = (FilterEnum)(int)filter;
            _filter = filterTypeEnum;
        }

        public byte[] Data
        {
            get
            {
                return new byte[] { Convert.ToByte((int)_filter) };
            }
        }

        public FilterEnum Filter
        {
            get { return _filter; }
        }
    }
}
