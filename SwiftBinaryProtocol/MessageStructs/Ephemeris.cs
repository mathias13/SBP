using System;

namespace SwiftBinaryProtocol.MessageStructs
{
    public struct Ephemeris
    {
        private double _tgd;

        private double _c_rs;

        private double _c_rc;

        private double _c_uc;

        private double _c_us;

        private double _c_ic;

        private double _c_is;

        private double _dn;

        private double _m0;

        private double _ecc;

        private double _sqrta;

        private double _omega0;

        private double _omegadot;

        private double _w;

        private double _inc;

        private double _inc_dot;

        private double _af0;

        private double _af1;

        private double _af2;

        private double _toe_tow;

        private ushort _toe_wn;

        private double _toc_tow;

        private ushort _toc_wn;

        private byte _valid;

        private byte _healthy;

        private byte _prn;

        private byte _iode;

        public Ephemeris(byte[] data)
        {
            _tgd = BitConverter.ToDouble(data, 0);
            _c_rs = BitConverter.ToDouble(data, 8);
            _c_rc = BitConverter.ToDouble(data, 16);
            _c_uc = BitConverter.ToDouble(data, 24);
            _c_us = BitConverter.ToDouble(data, 32);
            _c_ic = BitConverter.ToDouble(data, 40);
            _c_is = BitConverter.ToDouble(data, 48);
            _dn = BitConverter.ToDouble(data, 56);
            _m0 = BitConverter.ToDouble(data, 64);
            _ecc = BitConverter.ToDouble(data, 72);
            _sqrta = BitConverter.ToDouble(data, 80);
            _omega0 = BitConverter.ToDouble(data, 88);
            _omegadot = BitConverter.ToDouble(data, 96);
            _w = BitConverter.ToDouble(data, 104);
            _inc = BitConverter.ToDouble(data, 112);
            _inc_dot = BitConverter.ToDouble(data, 120);
            _af0 = BitConverter.ToDouble(data, 128);
            _af1 = BitConverter.ToDouble(data, 136);
            _af2 = BitConverter.ToDouble(data, 144);
            _toe_tow = BitConverter.ToDouble(data, 152);
            _toe_wn = BitConverter.ToUInt16(data, 160);
            _toc_tow = BitConverter.ToDouble(data, 168);
            _toc_wn = BitConverter.ToUInt16(data, 170);
            _valid = data[172];
            _healthy = data[173];
            _prn = data[174];
            _iode = data[175];
        }

        public double GroupDelay
        {
            get { return _tgd; }
        }

        public double SineHarmonicCorrectionOrbit
        {
            get { return _c_rs; }
        }

        public double CosineHarmonicCorrectionOrbit
        {
            get { return _c_rc; }
        }

        public double CosineHarmonicCorrectionLatitude
        {
            get { return _c_uc; }
        }

        public double SineHarmonicCorrectionLatitude
        {
            get { return _c_us; }
        }

        public double SineHarmonicCorrectionInclanation
        {
            get { return _c_is; }
        }

        public double CosineHarmonicCorrectionInclanation
        {
            get { return _c_ic; }
        }

        public double MeanMotionDifference
        {
            get { return _dn; }
        }

        public double MeanAnomaly
        {
            get { return _m0; }
        }

        public double Eccentricity
        {
            get { return _ecc; }
        }

        public double SquareRoot
        {
            get { return _sqrta; }
        }

        public double LongitudeAscendingNode
        {
            get { return _omega0; }
        }

        public double RateRightAscension
        {
            get { return _omegadot; }
        }

        public double ArgumentPerigee
        {
            get { return _w; }
        }

        public double Inclanation
        {
            get { return _inc; }
        }

        public double InclanationDerivative
        {
            get { return _inc_dot; }
        }

        public double ClockBias
        {
            get { return _af0; }
        }

        public double ClockDrift
        {
            get { return _af1; }
        }

        public double RateOfClockDrift
        {
            get { return _af2; }
        }

        public double TimeOfWeek
        {
            get { return _toe_tow; }
        }

        public ushort WeekNumber
        {
            get { return _toe_wn; }
        }

        public double ClockRefTimeOfWeek
        {
            get { return _toc_tow; }
        }

        public ushort ClockRefWeekNumber
        {
            get { return _toc_wn; }
        }
    
        public byte Valid
        {
            get { return _valid; }
        }

        public byte Healthy
        {
            get { return _healthy; }
        }

        public byte PRN
        {
            get { return _prn; }
        }

        public byte IODE
        {
            get { return _iode; }
        }
    }
}
