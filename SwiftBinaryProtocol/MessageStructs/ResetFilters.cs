using System;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct ResetFilters: IPayload
    {
        private SBP_Enums.ResetFilter _filter;

        public ResetFilters(SBP_Enums.ResetFilter filter)
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

        public SBP_Enums.ResetFilter Filter
        {
            get { return _filter; }
        }
    }
}
