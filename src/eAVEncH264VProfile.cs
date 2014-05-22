using System;

namespace MediaFoundation
{
    public enum eAVEncH264VProfile
    {
        Unknown = 0,
        Simple = 66,
        Base = 66,
        Main = 77,
        High = 100,
        _422 = 122,
        High10 = 110,
        _444 = 144,
        Extended = 88,
        ScalableBase = 83,
        ScalableHigh = 86,
        MultiviewHigh = 118,
        StereoHigh = 128,
        ConstrainedBase = 256,
        UCConstrainedHigh = 257,
        UCScalableConstrainedBase = 258,
        UCScalableConstrainedHigh = 259
    }
}
